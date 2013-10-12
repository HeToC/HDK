﻿using System;
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
        private DataObjectCollectionSource m_Docs;
        public DataObjectCollectionSource DOCS { get { return m_Docs; } set { m_Docs = value; RaisePropertyChanged(); } }

        public CharacterDatabase DB { get; set; }

        private Character m_SelectedCharacter;
        public Character SelectedCharacter { get { return m_SelectedCharacter; } set { m_SelectedCharacter = value; RaisePropertyChanged(); } }

        private Equipment m_SelectedEquipment;
        public Equipment SelectedEquipment { get { return m_SelectedEquipment; } set { m_SelectedEquipment = value; RaisePropertyChanged(); } }

        private IDataObjectFilter m_SelectedFilter;
        public IDataObjectFilter SelectedFilter
        {
            get { return m_SelectedFilter; }
            set { m_SelectedFilter = value; RaisePropertyChanged(); }
        }

        public IDataObjectSelector m_SelectedSelector;
        public IDataObjectSelector SelectedSelector
        {
            get { return m_SelectedSelector; }
            set { m_SelectedSelector = value; RaisePropertyChanged(); }
        }

        public IDataObjectSorter m_SelectedSorter;
        public IDataObjectSorter SelectedSorter
        {
            get { return m_SelectedSorter; }
            set { m_SelectedSorter = value; RaisePropertyChanged(); }
        }


        private Random rnd = new Random();
        public EntityDemoViewModel()
        {
            DOCS = new DataObjectCollectionSource();

            DB = new CharacterDatabase();

            DOCS.ItemsSource = DB.Characters;

            LoadDataCommand = new DelegateCommand((o) => LoadData());
        }

        public ICommand LoadDataCommand { get; set; }

        private async Task LoadData()
        {
            for(int i=0;i<200;i++)
            {
                var character = await DB.CreateEntityAsync<Character>();
                character.Name = System.Data.Fake.PersonName.GetName();
                await DB.AddEntityAsync(character);

                for(int e = 0;e<100;e++)
                {
                    var eq = await DB.CreateEntityAsync<Equipment>();
                    eq.Name = string.Format("Equupment {0}", e);
                    eq.CharacterId = character.Id;
                    await DB.AddEntityAsync(eq);

                    for(int egi = 0; egi < 100;egi++)
                    {
                        var gearItem = await DB.CreateEntityAsync<GearItem>();
                        gearItem.Name = string.Format("Item {0}", i + 1);
                        gearItem.Slot = (GearSlot)(System.Data.Fake.FakerRandom.Rand.Next((int)GearSlot.MIN, (int)GearSlot.MAX));
                        await DB.AddEntityAsync(gearItem);
                        
                        var egitem = await DB.CreateEntityAsync<EquipmentGearItem>(egi + 1);
                        egitem.EquipmentId = eq.Id;
                        egitem.GearItemId = gearItem.Id;
                        await DB.AddEntityAsync(egitem);
                    }
                }
            }
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

        public GearItem(DataObjectSet context, long id)
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
        public EquipmentGearItem(DataObjectSet context, long id)
            : base(context, id)
        {
        }

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

        public Equipment(DataObjectSet context, long id)
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

        public Character(DataObjectSet context, long id)
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
