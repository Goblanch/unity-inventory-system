using System;
using System.IO;
using UnityEngine;
using GB.Inventory.Application.Abstractions;

namespace GB.Inventory.Infrastructure.Persistence
{
    public interface IInventorySaveService
    {
        string GetSavePath(string slotName);
        bool Save(IInventoryService svc, string slotName, out string reason);
        bool Load(IInventoryService svc, string slotName, out string reason);
        bool Exists(string slotName);
    }

    /// <summary>
    /// JSON save/load to Application.persistentDataPath/GB.Inventory/.
    /// Uses atomic write (tmp+move) for robustness
    /// </summary>
    public sealed class JsonInventorySaveService : IInventorySaveService
    {
        private readonly string _rootDir;

        public JsonInventorySaveService(string folderName = "GB.Inventory")
        {
            _rootDir = Path.Combine(UnityEngine.Application.persistentDataPath, folderName);
        }

        public string GetSavePath(string slotName)
        {
            if (string.IsNullOrWhiteSpace(slotName)) slotName = "default";
            var safe = slotName.Replace("/", "_").Replace("\\", "_");
            return Path.Combine(_rootDir, $"{safe}.json");
        }

        public bool Exists(string slotName) => File.Exists(GetSavePath(slotName));

        public bool Save(IInventoryService svc, string slotName, out string reason)
        {
            reason = null;

            try
            {
                Directory.CreateDirectory(_rootDir);

                var dto = InventorySerializer.Capture(svc);
                var json = JsonUtility.ToJson(dto, true);

                var path = GetSavePath(slotName);
                var tmp = path + ".tmp";

                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);

                return true;
            }
            catch (Exception ex)
            {
                reason = $"Save failed: {ex.Message}";
                return false;
            }
        }

        public bool Load(IInventoryService svc, string slotName, out string reason)
        {
            reason = null;

            try
            {
                var path = GetSavePath(slotName);
                if (!File.Exists(path))
                {
                    reason = "Save file not found";
                    return false;
                }

                var json = File.ReadAllText(path);
                var dto = JsonUtility.FromJson<InventorySaveDTO>(json);
                if (dto == null)
                {
                    reason = "Invalid JSON";
                    return false;
                }

                return InventorySerializer.Restore(svc, dto, out reason);
            }
            catch (Exception ex)
            {
                reason = $"Load failed: {ex.Message}";
                return false;
            }
        }

    }
}