using System;
using System.IO;
using UnityEngine;

namespace PrototypeRT
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        private const string SaveFileName = "prototype_rt_save.json";

        public static SaveManager Instance => _instance;
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        [Serializable]
        private class SaveData
        {
            public int gold;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public int LoadGold(int fallbackGold)
        {
            if (!File.Exists(SavePath))
                return Mathf.Max(0, fallbackGold);

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return Mathf.Max(0, data != null ? data.gold : fallbackGold);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SaveManager: 저장 파일을 읽지 못해 기본 골드를 사용합니다. {ex.Message}");
                return Mathf.Max(0, fallbackGold);
            }
        }

        public void SaveGold(int gold)
        {
            try
            {
                // v0.1에서는 영구 진행 검증에 필요한 골드만 저장한다. 인벤토리/장비 저장은 후순위다.
                SaveData data = new() { gold = Mathf.Max(0, gold) };
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SaveManager: 골드를 저장하지 못했습니다. {ex.Message}");
            }
        }
    }
}
