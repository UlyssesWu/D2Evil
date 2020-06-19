using System;
using System.Collections.Generic;

namespace D2Evil.Peacock
{
    [Serializable]
    public class CharacterData
    {
        [Serializable]
        public class LpkData
        {
            public LpkData()
            {
                this.charList = new List<CharacterData>();
            }

            public int index;

            public string type;

            public int listId;

            public string title;

            public string keyId;

            public bool encrypt;

            public string zipFileName;

            public string version;

            public string preview;

            public List<CharacterData> charList;

            public WorkshopData workshopItem;
        }

        public CharacterData(LpkData lpk)
        {
            this.parent = lpk;
            this.cosList = new List<CostumeData>();
        }

        public string id;

        public string name;

        public string avatar;

        public List<CostumeData> cosList;

        [NonSerialized]
        public LpkData parent;
    }

    [Serializable]
    public class CostumeData
    {
        public CostumeData(CharacterData character)
        {
            this.parent = character;
        }

        public int id;

        public string name;

        public string thumbnail;

        public string jsonFilePath;

        [NonSerialized]
        public CharacterData parent;
    }

    [Serializable]
    public class CharSaveData
    {
        public CharSaveData()
        {
        }

        public float x;

        public float y;

        public float z;

        public float viewportX = -1f;

        public float viewportY = -1f;

        public float viewportZ = -1f;

        public float scale = 1f;

        public bool mirror;

        public float rotate;

        public int motionEnable = 1;

        public int voiceEnable = 1;

        public float voiceVolume = 1f;

        public int voiceTextEnable = 1;

        public int type;

        public string configFilePath;

        public string zipFilePath;

        public string jsonFilePath;

        public int compatMode;

        public int bubbleX;

        public int bubbleY;

        public int expression = -1;

        public bool bubbleLock;
    }

    [Serializable]
    public class WorkshopData
    {
        public WorkshopData()
        {
        }

        public string lpkFile;

        public string file;

        public int type;

        public int stereoMode;

        public string previewFile;

        public string fileId;

        public string title;

        public string author;

        public string description;

        public string metaData;

        public int vote;
    }
}
