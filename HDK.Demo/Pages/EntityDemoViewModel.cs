using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HDK.Demo.Pages
{
    [ExportViewModel("#Demo #Entity")]
    public class EntityDemoViewModel : ViewModelBase
    {
        public CharacterDatabase DB { get; set; }

        public Character SelectedCharacter { get; set; }

        private Random rnd = new Random();
        public EntityDemoViewModel()
        {
            DB = new CharacterDatabase();

            try
            {
                for (int i = 0; i < 100; i++)
                {
                    
                    var gearItem = DB.CreateEntity<GearItem>(i);
                    gearItem.Name = string.Format("Item {0}", i + 1);
                    gearItem.Slot = (GearSlot)(System.Data.Fake.FakerRandom.Rand.Next((int)GearSlot.MIN, (int)GearSlot.MAX));
                }

                for (int i = 200; i < 250; i++)
                {
                    var character = DB.CreateEntity<Character>(i);
                    character.Name = System.Data.Fake.PersonName.GetName();
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }

            //for (int ic = 0; ic < 100; ic++)
            //{
            //    var c = new Character(DB, ic + 1)
            //    {
            //        Name = System.Data.Fake.PersonName.GetName()
            //    };
            //    for(int i=0;i<10;i++)
            //    {
            //        DB.Equipments.Add(new Equipment(DB, i + 1)
            //            {
            //                Name = string.Format("Equipment {0}", i + 1),
            //                CharacterId = ic
            //            });
            //    }
            //}


        }
    }

    #region DataObjects

 
    public sealed class CharacterDatabase : DataObjectSet
    {
        public CharacterDatabase ()
        {
            Characters = RegisterEntityCollection<Character>();
            GearItems = RegisterEntityCollection<GearItem>();
            Equipments = RegisterEntityCollection<Equipment>();
            EquipmentGearItems = RegisterEntityCollection<EquipmentGearItem>();
        }
 
        public DataObjectCollection<Character> Characters { get; private set; }
        public DataObjectCollection<GearItem> GearItems { get; private set; }
        public DataObjectCollection<Equipment> Equipments { get; private set; }
        public DataObjectCollection<EquipmentGearItem> EquipmentGearItems { get; private set; } 
    }



    public enum GearSlot : int
    {
        MIN = 0,
        Head = 0,
        Shoulders = 1,
        Chest = 2,
        Hands = 3,
        Legs = 4,
        Feet = 5,
        Back = 6,
        EarringLeft = 7,
        EarringRight = 8,
        RingLeft = 9,
        RingRight = 10,
        BraceletLeft = 11,
        BraceletRight = 12,
        Necklace = 13,
        Pocket = 14,
        FirstHand = 15,
        SecondHand = 16,
        Ranged = 17,
        Tool = 18,
        ClassItem = 19,
        MAX = 19
    }

    public class GearItem : DataObject
    {
        public const string GearItemEquipmentGearItemRelation = "GearItemEquipmentGearItemRelation";

        protected GearItem(DataObjectSet context, long id)
            : base(context, id)
        {
        }

        public DataObjectCollection<EquipmentGearItem> EquipmentGearItems
        {
            get
            {
                var context = Context;
                if (context == null) return null;
                return context.GetEntitiesByForeignKey<EquipmentGearItem>(GearItemEquipmentGearItemRelation, Id);
            }
        }

        private GearSlot _slot;

        public GearSlot Slot
        {
            get { return _slot; }
            set
            {
                if (_slot == value) return;
                _slot = value;
                RaisePropertyChanged("Slot");
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private byte[] _icon;

        public byte[] Icon
        {
            get { return _icon; }
            set
            {
                if (_icon == value) return;
                _icon = value;
                RaisePropertyChanged("Icon");
            }
        }

        private byte[] _Image;

        public byte[] Image
        {
            get { return _Image; }
            set
            {
                if (_Image == value) return;
                _Image = value;
                RaisePropertyChanged("Image");
            }
        }
    }

    public class EquipmentGearItem : DataObject
    {
        private long _equipmentId;

        public long EquipmentId
        {
            get { return _equipmentId; }
            set
            {
                if (_equipmentId != value)
                {
                    long oldKey = _equipmentId;
                    _equipmentId = value;
                    Context.SetForeignKey(Equipment.EquipmentEquipmentGearItemRelation, _equipmentId, oldKey, this);
                    RaisePropertyChanged("EquipmentId");
                }
            }
        }

        public Equipment Equipment
        {
            get { return FindInContext<Equipment>(_equipmentId); }
        }

        private long _gearItemId;

        public long GearItemId
        {
            get { return _gearItemId; }
            set
            {
                if (_gearItemId != value)
                {
                    long oldKey = _gearItemId;
                    _gearItemId = value;
                    Context.SetForeignKey(GearItem.GearItemEquipmentGearItemRelation, _gearItemId, oldKey, this);
                    RaisePropertyChanged("GearItemId");
                }
            }
        }

        public GearItem GearItem
        {
            get { return FindInContext<GearItem>(_gearItemId); }
        }
    }

    public class Equipment : DataObject
    {
        public const string EquipmentEquipmentGearItemRelation = "EquipmentEquipmentGearItemRelation";

        protected Equipment(DataObjectSet context, long id)
            : base(context, id)
        {
        }

        public DataObjectCollection<EquipmentGearItem> EquipmentGearItems
        {
            get
            {
                var context = Context;
                if (context == null) return null;
                return context.GetEntitiesByForeignKey<EquipmentGearItem>
                       (EquipmentEquipmentGearItemRelation, Id);
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private long _characterId;

        public long CharacterId
        {
            get { return _characterId; }
            set
            {
                if (_characterId != value)
                {
                    long oldKey = _characterId;
                    _characterId = value;
                    Context.SetForeignKey(Character.CharacterEquipmentRelation, _characterId, oldKey, this);
                    RaisePropertyChanged("CharacterId");
                }
            }
        }

        public Character Character
        {
            get { return FindInContext<Character>(_characterId); }
        }
    }

    public class Character : DataObject
    {
        public const string CharacterEquipmentRelation =
                            "CharacterEquipmentRelation";

        protected Character(DataObjectSet context, long id)
            : base(context, id)
        {
        }

        public DataObjectCollection<Equipment> Equipments
        {
            get
            {
                var context = Context;
                if (context == null) return null;
                return context.GetEntitiesByForeignKey<Equipment>(CharacterEquipmentRelation, Id);
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private int _level;

        public int Level
        {
            get { return _level; }
            set
            {
                if (_level == value) return;
                _level = value;
                RaisePropertyChanged("Level");
            }
        }
    }
    
    #endregion

}
