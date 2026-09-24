using NUnit.Framework;
using UnityEngine;

namespace MobilityLabVR.Tests
{
    public sealed class ScenarioDeterminismTests
    {
        [Test]
        public void StableSeed_SameInputs_ReturnSameValue()
        {
            int first = SessionContext.StableSeed("P042", ScenarioKind.SuddenPedestrian, 2);
            int second = SessionContext.StableSeed("P042", ScenarioKind.SuddenPedestrian, 2);
            int different = SessionContext.StableSeed("P042", ScenarioKind.SuddenPedestrian, 3);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(different, Is.Not.EqualTo(first));
        }

        [Test]
        public void ScenarioReset_ReusesConfiguredSeedAndClearsActivation()
        {
            GameObject managerObject = new GameObject("Scenario Test Manager");
            ScenarioManager manager = managerObject.AddComponent<ScenarioManager>();
            var definitions = DefaultScenarioFactory.CreateBuiltInDefinitions();
            manager.Configure(definitions, null, null, null, null, null);
            SessionContext.SelectedScenario = ScenarioKind.VehicleFailsToYield;

            manager.ResetForTrial(12345);
            int firstSeed = manager.CurrentSeed;
            manager.ActivateScenarioHazard();
            manager.ResetForTrial(12345);

            Assert.That(manager.Current.Kind, Is.EqualTo(ScenarioKind.VehicleFailsToYield));
            Assert.That(manager.CurrentSeed, Is.EqualTo(firstSeed));
            Assert.That(manager.HazardActivated, Is.False);
            Object.DestroyImmediate(managerObject);
            foreach (ScenarioDefinition definition in definitions) Object.DestroyImmediate(definition);
        }
    }
}
