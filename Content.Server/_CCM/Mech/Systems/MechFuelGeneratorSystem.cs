using Content.Server._CCM.Mech.Components;
using Content.Server.Power.Generator;
using Content.Shared.Materials;
using Content.Shared._CCM.Mech;
using Content.Shared._CCM.Mech.Components;
using Content.Shared._CCM.Mech.EntitySystems;
using Content.Shared.Power.Generator;
using Robust.Shared.GameObjects;

namespace Content.Server._CCM.Mech.Systems;

/// <summary>
/// Bridges mech FuelGenerator-based modules to the mech battery by consuming fuel via the standard
/// generator events and adding the module's chargeRate into the per-tick recharge accumulator.
/// </summary>
public sealed partial class MechFuelGeneratorBridgeSystem : EntitySystem
{
    [Dependency] private readonly GeneratorSystem _generator = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CCMMechGeneratorModuleComponent, MechEquipmentUiMessageRelayEvent>(OnMechGeneratorMessage);
        SubscribeLocalEvent<CCMMechGeneratorModuleComponent, MechEquipmentUiStateReadyEvent>(OnUiStateReady);
    }

    private void OnMechGeneratorMessage(EntityUid uid, CCMMechGeneratorModuleComponent component, MechEquipmentUiMessageRelayEvent args)
    {
        if (args.Message is not MechGeneratorEjectFuelMessage)
            return;

        if (!TryComp<FuelGeneratorComponent>(uid, out _))
            return;

        _generator.EmptyGenerator(uid);
    }

    private void OnUiStateReady(EntityUid uid, CCMMechGeneratorModuleComponent gen, MechEquipmentUiStateReadyEvent args)
    {
        var ui = new MechGeneratorUiState();

        // Read live telemetry written by generator systems each tick
        if (TryComp<CCMMechEnergyAccumulatorComponent>(uid, out var telem))
        {
            ui.ChargeCurrent = telem.Current;
            ui.ChargeMax = telem.Max;
        }

        if (gen.GenerationType == MechGenerationType.FuelGenerator)
        {
            if (TryComp<SolidFuelGeneratorAdapterComponent>(uid, out var solid))
            {
                var amount = _materialStorage.GetMaterialAmount(uid, solid.FuelMaterial);
                amount += (int) MathF.Floor(solid.FractionalMaterial);

                if (TryComp<MaterialStorageComponent>(uid, out var storage))
                {
                    ui.HasFuel = true;
                    ui.FuelCapacity = storage.StorageLimit ?? 0;
                }

                ui.FuelName = solid.FuelMaterial;
                ui.FuelAmount = amount;
            }
        }

        args.States[GetNetEntity(uid)] = ui;
    }

	public override void Update(float frameTime)
	{
		var query = EntityQueryEnumerator<CCMMechComponent>();
		while (query.MoveNext(out var mechUid, out var mech))
		{
			if (!TryComp<CCMMechEnergyAccumulatorComponent>(mechUid, out var acc))
				acc = EnsureComp<CCMMechEnergyAccumulatorComponent>(mechUid);

			foreach (var module in mech.ModuleContainer.ContainedEntities)
			{
				if (!TryComp<CCMMechGeneratorModuleComponent>(module, out var gen))
					continue;
				if (gen.GenerationType != MechGenerationType.FuelGenerator)
					continue;

				var telem = EnsureComp<CCMMechEnergyAccumulatorComponent>(module);
				telem.Max = 0f;
				telem.Current = 0f;

				if (!TryComp<FuelGeneratorComponent>(module, out var fuelGen))
					continue;

				// max output is the configured target power
				telem.Max = fuelGen.TargetPower;

				var availableFuel = _generator.GetFuel(module);
				if (availableFuel <= 0 || _generator.GetIsClogged(module))
					continue;

				var eff = 1 / SharedGeneratorSystem.CalcFuelEfficiency(fuelGen.TargetPower, fuelGen.OptimalPower, fuelGen);
				var burn = fuelGen.OptimalBurnRate * frameTime * eff;
				RaiseLocalEvent(module, new GeneratorUseFuel(burn));

				// Current contribution equals target power when fuel is available
				var current = fuelGen.TargetPower;
				acc.PendingRechargeRate += current;
				telem.Current = current;
			}

			UpdateMechUI(mechUid);
		}
	}

    private void UpdateMechUI(EntityUid uid)
    {
        RaiseLocalEvent(uid, new UpdateMechUiEvent());
    }
}
