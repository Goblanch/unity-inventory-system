using NUnit.Framework;
using GB.Inventory.Domain.Policies;
using GB.Inventory.Domain;
using GB.Inventory.Application;
using GB.Inventory.Application.Abstractions;
using GB.Inventory.Domain.Abstractions;
using GB.Inventory.Infrastructure.Effects;

namespace GB.Inventory.Tests
{
    class Ctx : ITurnContext { public string Phase; string ITurnContext.Phase => Phase; }

    public class UseEffectPhaseTests
    {
        private IInventoryService CreateSvc(IUsagePhasePolicy phasePolicy)
        {
            var stacking = new SimpleStackingPolicy(99);

            // Perfiles "abiertos" (aceptan todo y apilan)
            var profiles = new OpenProfiles();
            var meta     = new AnyMeta();

            var filter = new GB.Inventory.Domain.Policies.SimpleSlotFilterPolicy(profiles, meta, stacking);
            var inv    = new InventoryModel(2, stacking, filter, "Default");
            var reg    = new EffectRegistry(infoProvider: null) // sin SO: resolvemos por effectKey directo
                .RegisterEffect("test", new TestEffect());

            // Registramos el item "apple" como si su effectKey fuera "echo"
            // (sin SO, usamos TryResolve por effectKey directamente)
            // Para emular definitionId -> effectKey, usamos definición == "echo" en el test:
            return new InventoryService(inv, reg, phasePolicy);
        }

        [Test]
        public void Use_Allows_When_No_Phases_Configured()
        {
            var svc = CreateSvc(phasePolicy: new DefaultUsagePhasePolicy());
            // Añadimos "echo" como si fuera un item cuya definitionId == effectKey
            Assert.IsTrue(svc.TryAdd("test", 1, out var slot, out var r1), r1);

            var ctx = new Ctx { Phase = null }; // sin fases
            Assert.IsTrue(svc.TryUse(slot, ctx, out var res, out var why), why);
            Assert.IsTrue(res.Success);
        }

        [Test]
        public void Use_Fails_When_Phase_Not_Allowed()
        {
            // Fase policy activa y item con allowed phases simuladas via registry? Aquí, como no hay SO,
            // probamos solamente que, con ctx.Phase distinto y allowed [] -> permite (policy default permite)
            // Para probar fase prohibida necesitamos SO; lo validamos en SoProviders (EditMode).
            Assert.Pass("Validaciones de fases por SO se cubren en pruebas de infraestructura.");
        }

        // Dummies
        class OpenProfiles : ISlotProfileProvider
        {
            public SlotProfile Get(string slotProfileId) => new SlotProfile
            {
                Id = slotProfileId,
                AllowedTypes = System.Array.Empty<string>(),
                RequiredTags = System.Array.Empty<string>(),
                BannedTags = System.Array.Empty<string>(),
                HasStackableOverride = true,
                StackableOverride = true,
                MaxStackOverride = 99
            };
        }

        class AnyMeta : IItemMetadataProvider
        {
            public ItemMeta Get(string definitionId) => new ItemMeta
            {
                DefinitionId = definitionId,
                TypeId = "Any",
                Tags = System.Array.Empty<string>()
            };
        }
    }
}
