using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Winda
{
    public enum ElevatorState
    {
        Idle,
        Moving,
        DoorOpening,
        DoorOpen,
        DoorClosing,
        Emergency
    }

    public enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    public class ElevatorService
    {
        private readonly int _totalFloors = 6;
        private readonly int _floorTransitionTime = 1000; 
        private readonly int _doorOpenTime = 2000; 
        private readonly int _doorCloseTime = 1500;

        private int _currentFloor = 1;
        private ElevatorState _state = ElevatorState.Idle;
        private DoorState _doorState = DoorState.Closed;
        private Queue<int> _floorQueue = new Queue<int>();
        private HashSet<int> _pendingFloors = new HashSet<int>();
        private bool _isEmergency = false;

        public event Action<int> OnFloorChanged;
        public event Action<DoorState> OnDoorStateChanged;
        public event Action<ElevatorState> OnStateChanged;
        public event Action<string> OnLogMessage;
        public event Action<IEnumerable<int>> OnQueueUpdated;

        public int CurrentFloor => _currentFloor;
        public ElevatorState State => _state;
        public DoorState DoorState => _doorState;
        public IEnumerable<int> FloorQueue => _floorQueue.ToList();

        public ElevatorService()
        {
            LogMessage("System uruchomiony. Winda na piętrze 1");
        }

        public void CallElevator(int floor)
        {
            if (floor < 1 || floor > _totalFloors)
            {
                LogMessage($"❌ Błąd: Piętrze {floor} nie istnieje!");
                return;
            }

            if (_isEmergency)
            {
                LogMessage($"⚠️ Winda w stanie NADZWYCZAJNYM! Niemożliwe wezwanie piętra {floor}");
                return;
            }

            if (_pendingFloors.Contains(floor))
            {
                LogMessage($"ℹ️ Piętrze {floor} już w kolejce");
                return;
            }

            if (floor == _currentFloor && _state == ElevatorState.Idle && _doorState == DoorState.Open)
            {
                LogMessage($"ℹ️ Już na piętrze {floor}. Drzwi otwarte");
                return;
            }

            _floorQueue.Enqueue(floor);
            _pendingFloors.Add(floor);
            LogMessage($"📍 Zażądano piętra {floor} - kolejka: {string.Join(", ", _floorQueue)}");
            OnQueueUpdated?.Invoke(_floorQueue.ToList());

            if (_state == ElevatorState.Idle)
            {
                _ = StartMoving();
            }
        }

        public void EmergencyStop()
        {
            if (_isEmergency)
            {
                LogMessage("⚠️ Już w stanie nadzwyczajnym!");
                return;
            }

            _isEmergency = true;
            _floorQueue.Clear();
            _pendingFloors.Clear();
            SetState(ElevatorState.Emergency);
            SetDoorState(DoorState.Closed);
            LogMessage("🚨 ALARM! Winda zatrzymana w stanie nadzwyczajnym na piętrze " + _currentFloor);
        }

        public void CancelEmergency()
        {
            if (!_isEmergency)
            {
                LogMessage("ℹ️ Nie ma aktywnego alarmu");
                return;
            }

            _isEmergency = false;
            SetState(ElevatorState.Idle);
            LogMessage("✅ Stan nadzwyczajny anulowany. Winda gotowa do pracy");
        }

        private async Task StartMoving()
        {
            if (_floorQueue.Count == 0 || _isEmergency)
                return;

            while (_floorQueue.Count > 0 && !_isEmergency)
            {
                int targetFloor = _floorQueue.Peek();

                await MoveToFloor(targetFloor);

                await OpenDoors();

                await Task.Delay(_doorOpenTime);

                await CloseDoors();

                _floorQueue.Dequeue();
                _pendingFloors.Remove(targetFloor);
                OnQueueUpdated?.Invoke(_floorQueue.ToList());
            }

            SetState(ElevatorState.Idle);
            LogMessage("✅ Winda w gotowości na piętrze " + _currentFloor);
        }

        private async Task MoveToFloor(int targetFloor)
        {
            if (targetFloor == _currentFloor)
                return;

            SetState(ElevatorState.Moving);
            int direction = targetFloor > _currentFloor ? 1 : -1;
            string directionText = direction > 0 ? "⬆️ w górę" : "⬇️ w dół";

            LogMessage($"🚀 Winda jedzie {directionText} do piętra {targetFloor}");

            while (_currentFloor != targetFloor && !_isEmergency)
            {
                await Task.Delay(_floorTransitionTime);
                _currentFloor += direction;
                OnFloorChanged?.Invoke(_currentFloor);
                LogMessage($"📍 Winda na piętrze {_currentFloor}");
            }
        }

        private async Task OpenDoors()
        {
            SetDoorState(DoorState.Opening);
            LogMessage($"🚪 Otwieranie drzwi na piętrze {_currentFloor}");

            await Task.Delay(_doorCloseTime);

            SetDoorState(DoorState.Open);
            LogMessage($"✅ Drzwi otwarte na piętrze {_currentFloor}");
            SetState(ElevatorState.DoorOpen);
        }

        private async Task CloseDoors()
        {
            SetDoorState(DoorState.Closing);
            LogMessage($"🚪 Zamykanie drzwi na piętrze {_currentFloor}");

            await Task.Delay(_doorCloseTime);

            SetDoorState(DoorState.Closed);
            LogMessage($"✅ Drzwi zamknięte na piętrze {_currentFloor}");
        }

        private void SetState(ElevatorState newState)
        {
            if (_state != newState)
            {
                _state = newState;
                OnStateChanged?.Invoke(_state);
            }
        }

        private void SetDoorState(DoorState newState)
        {
            if (_doorState != newState)
            {
                _doorState = newState;
                OnDoorStateChanged?.Invoke(_doorState);
            }
        }

        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
