using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Noggog;



namespace SmeltingRecipes
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {

            Dictionary<FormKey, COBJ> armorDepo = new Dictionary<FormKey, COBJ>();
            Dictionary<FormKey, COBJ> weaponDepo = new Dictionary<FormKey, COBJ>();
            foreach (var COBJGetter in state.LoadOrder.PriorityOrder.ConstructibleObject().WinningOverrides())
            {
                if (!weaponDepo.ContainsKey(COBJGetter.CreatedObject.FormKey) && !armorDepo.ContainsKey(COBJGetter.CreatedObject.FormKey))
                {
                    if (COBJGetter.WorkbenchKeyword.TryResolve(state.LinkCache, out var keyword)
                    && keyword.EditorID?.Contains("forge", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (COBJGetter.CreatedObject.TryResolve(state.LinkCache, out var stuff))
                        {
                            switch (stuff)
                            {
                                case IWeaponGetter weapon:
                                    if (COBJGetter.Items is not null)
                                    {
                                        COBJ weap = new COBJ();
                                        bool go = false;
                                        ushort count = 0;
                                        foreach (IContainerEntryGetter? item in COBJGetter.Items)
                                        {
                                            if (item.Item.Item.TryResolve(state.LinkCache, out var EDID) && EDID.EditorID?.Contains("ingot", StringComparison.OrdinalIgnoreCase) == true)
                                            {
                                                if (item.Item.Count > count)
                                                {
                                                    go = true;
                                                    weap.ingot = item.Item.Item.FormKey;
                                                    weap.amount = (ushort)item.Item.Count;
                                                    count = (ushort)item.Item.Count;
                                                }
                                            }
                                        }
                                        if (go == true) { weaponDepo.Add(COBJGetter.CreatedObject.FormKey, weap); }
                                    }
                                    break;
                                case IArmorGetter armor:
                                    if (COBJGetter.Items is not null)
                                    {
                                        COBJ armo = new COBJ();
                                        bool go = false;
                                        ushort count = 0;
                                        foreach (IContainerEntryGetter? item in COBJGetter.Items)
                                        {
                                            if (item.Item.Item.TryResolve(state.LinkCache, out var EDID) && EDID.EditorID?.Contains("ingot", StringComparison.OrdinalIgnoreCase) == true)
                                            {

                                                if (item.Item.Count > count)
                                                {
                                                    go = true;
                                                    armo.ingot = item.Item.Item.FormKey;
                                                    armo.amount = (ushort)item.Item.Count;
                                                    count = (ushort)item.Item.Count;
                                                }
                                            }
                                        }
                                        if (go == true) { armorDepo.Add(COBJGetter.CreatedObject.FormKey, armo); }
                                    }
                                    break;
                                default: break;
                            }
                        }
                    }
                }

            }

            foreach (var armorGetter in state.LoadOrder.PriorityOrder.Armor().WinningOverrides())
            {
                COBJ result = new COBJ();
                if (armorDepo.ContainsKey(armorGetter.FormKey))
                {
                    result = armorDepo[armorGetter.FormKey];
                }
                else if (armorDepo.ContainsKey(armorGetter.TemplateArmor.FormKey))
                {
                    result = armorDepo[armorGetter.TemplateArmor.FormKey];
                }
                else {
                    continue;
                }

                var recipe = state.PatchMod.ConstructibleObjects.AddNew();
                recipe.EditorID = "RecipeSmelt" + armorGetter.EditorID;
                if (recipe.Items == null)
                {
                    recipe.Items = new();
                }
                ContainerEntry temp = new ContainerEntry();
                temp.Item.Item.FormKey = armorGetter.FormKey;
                temp.Item.Count = 1;
                recipe.Items.SetTo(temp);

                recipe.CreatedObject.SetTo(result.ingot);
                recipe.CreatedObjectCount = result.amount;

                recipe.WorkbenchKeyword.SetTo(Skyrim.Keyword.CraftingSmelter);
            }
            foreach (var weaponGetter in state.LoadOrder.PriorityOrder.Weapon().WinningOverrides())
            {
                COBJ result = new COBJ();
                if (weaponDepo.ContainsKey(weaponGetter.FormKey))
                {
                    result = weaponDepo[weaponGetter.FormKey];
                }
                else if (weaponDepo.ContainsKey(weaponGetter.Template.FormKey))
                {
                    result = weaponDepo[weaponGetter.Template.FormKey];
                }
                else {
                    continue;
                }

                var recipe = state.PatchMod.ConstructibleObjects.AddNew();
                recipe.EditorID = "RecipeSmelt" + weaponGetter.EditorID;
                if (recipe.Items == null)
                {
                    recipe.Items = new();
                }
                ContainerEntry temp = new ContainerEntry();
                temp.Item.Item.FormKey = weaponGetter.FormKey;
                temp.Item.Count = 1;
                recipe.Items.SetTo(temp);

                recipe.CreatedObject.SetTo(result.ingot);
                recipe.CreatedObjectCount = result.amount;

                recipe.WorkbenchKeyword.SetTo(Skyrim.Keyword.CraftingSmelter);
            }
        }
    }
}
