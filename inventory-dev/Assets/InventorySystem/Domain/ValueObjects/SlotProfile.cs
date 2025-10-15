namespace GB.Inventory.Domain
{
    public struct SlotProfile
    {
        public string Id;

        // Filtros
        public string[] AllowedTypes; // si vacío = cualquier tipo
        public string[] RequiredTags; // todos deben estar presentes (si no vacío)
        public string[] BannedTags; // ninguno de estos debe estar

        // Overrides de stack
        public bool HasStackableOverride; // si true, fuerza comportamiento
        public bool StackableOverride; // true / false
        public int MaxStackOverride; // >0 para override; 0 = sin override
    }
}