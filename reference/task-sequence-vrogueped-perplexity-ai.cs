using System;
using System.Linq;
using GTA;
using GTA.Native;

public class PedController : Script
{
    private Ped _targetPed;
    private Ped _secondPed;
    private Vehicle _targetVehicle;
    private TaskSequence _mainSequence;
    private Vector3 _airportEntrance = new Vector3(-1337.0f, -3044.0f, 13.9f);

    public PedController()
    {
        Tick += OnTick;
        Aborted += OnAborted;
    }

    void InitializeSequence()
    {
        _targetPed = World.GetNearbyPeds(Game.Player.Character, 50f)
            .Where(p => p.IsAlive && !p.IsInVehicle() && p.IsHuman)
            .OrderBy(p => p.Position.DistanceTo(Game.Player.Character.Position))
            .FirstOrDefault();

        if (_targetPed == null) return;

        _secondPed = World.GetNearbyPeds(_targetPed, 50f)
            .Where(p => p != _targetPed && p.IsAlive)
            .OrderBy(p => p.Position.DistanceTo(_targetPed.Position))
            .FirstOrDefault();

        if (_secondPed == null) return;

        _mainSequence = new TaskSequence();

        // Stage 1: Look at target for 4 seconds
        _mainSequence.AddTask.LookAt(_secondPed, 4000);
        _mainSequence.AddTask.Wait(4000);

        // Stage 2: Run to target
        _mainSequence.AddTask.RunTo(_secondPed.Position, false);
        _mainSequence.AddTask.WaitUntil(() => 
            _targetPed.Position.DistanceTo(_secondPed.Position) < 3f);

        // Stage 3: Aim and shoot
        _mainSequence.AddTask.Weapon(WeaponHash.Pistol);
        _mainSequence.AddTask.AimAt(_secondPed, 1000);
        _mainSequence.AddTask.Wait(1000);
        _mainSequence.AddTask.ShootAt(_secondPed, 5000);
        _mainSequence.AddTask.Wait(5000);

        // Stage 4: Enter vehicle and drive
        _mainSequence.AddTask.Perform(() => {
            _targetVehicle = World.GetNearbyVehicles(_targetPed.Position, 30f)
                .OrderBy(v => v.Position.DistanceTo(_targetPed.Position))
                .FirstOrDefault();
            return _targetVehicle;
        });
        _mainSequence.AddTask.EnterVehicle(_targetVehicle, VehicleSeat.Driver);
        _mainSequence.AddTask.DriveTo(_targetVehicle, _airportEntrance, 30f, 50f, DrivingStyle.Aggressive);

        _mainSequence.Close();
        _targetPed.Task.PerformSequence(_mainSequence);
    }

    void OnTick(object sender, EventArgs e)
    {
        if (_targetPed == null || !_targetPed.IsAlive)
        {
            InitializeSequence();
        }
    }

    void OnAborted(object sender, EventArgs e)
    {
        _mainSequence?.Dispose();
    }
}
