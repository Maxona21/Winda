using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Winda
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly ElevatorService _elevatorService;

        private int _currentFloor = 1;
        private string _doorStateText = "Zamknięte";
        private string _elevatorStateText = "Gotowość";
        private string _doorStateColor = "#FF0000";

        public ObservableCollection<int> FloorQueue { get; }

        public int CurrentFloor
        {
            get => _currentFloor;
            set
            {
                if (_currentFloor != value)
                {
                    _currentFloor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DoorStateText
        {
            get => _doorStateText;
            set
            {
                if (_doorStateText != value)
                {
                    _doorStateText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ElevatorStateText
        {
            get => _elevatorStateText;
            set
            {
                if (_elevatorStateText != value)
                {
                    _elevatorStateText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DoorStateColor
        {
            get => _doorStateColor;
            set
            {
                if (_doorStateColor != value)
                {
                    _doorStateColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CallFloorCommand { get; }

        public MainWindowViewModel()
        {
            _elevatorService = new ElevatorService();
            FloorQueue = new ObservableCollection<int>();

            CallFloorCommand = new RelayCommand(o =>
            {
                if (int.TryParse(o?.ToString(), out int floor))
                {
                    _elevatorService.CallElevator(floor);
                }
            });

            _elevatorService.OnFloorChanged += floor => CurrentFloor = floor;
            _elevatorService.OnDoorStateChanged += UpdateDoorState;
            _elevatorService.OnStateChanged += UpdateElevatorState;
            _elevatorService.OnQueueUpdated += UpdateFloorQueue;
        }

        private void UpdateDoorState(DoorState doorState)
        {
            switch (doorState)
            {
                case DoorState.Closed:
                    DoorStateText = "Zamknięte";
                    DoorStateColor = "#FF0000";
                    break;
                case DoorState.Opening:
                    DoorStateText = "Otwieranie...";
                    DoorStateColor = "#FFA500";
                    break;
                case DoorState.Open:
                    DoorStateText = "Otwarte";
                    DoorStateColor = "#00AA00";
                    break;
                case DoorState.Closing:
                    DoorStateText = "Zamykanie...";
                    DoorStateColor = "#FFA500";
                    break;
            }
        }

        private void UpdateElevatorState(ElevatorState elevatorState)
        {
            switch (elevatorState)
            {
                case ElevatorState.Idle:
                    ElevatorStateText = "Gotowość";
                    break;
                case ElevatorState.Moving:
                    ElevatorStateText = "W ruchu";
                    break;
                case ElevatorState.DoorOpen:
                    ElevatorStateText = "Drzwi otwarte";
                    break;
                case ElevatorState.DoorOpening:
                    ElevatorStateText = "Otwieranie drzwi";
                    break;
                case ElevatorState.DoorClosing:
                    ElevatorStateText = "Zamykanie drzwi";
                    break;
            }
        }

        private void UpdateFloorQueue(System.Collections.Generic.IEnumerable<int> floors)
        {
            FloorQueue.Clear();
            foreach (var floor in floors)
            {
                FloorQueue.Add(floor);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
