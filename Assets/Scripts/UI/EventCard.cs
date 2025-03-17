using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EventCard : MonoBehaviour
    {
        //public TMP_Text EventTitle;
        public TMP_Text EventDescriptionBoxDummy;
        public TMP_Text EventDescription;
        public ScrollRect EventDescriptionScroll;
        public ButtonList EventOptionsList;
        public Image EventPicture;
        //public AudioSource EventSound;
        
        public GameEvent Data;
        private Option _selectedOption;
        private int _tldrPosition;
        private GameEvent _tldrData;

        private void Awake()
        {
            GameManager.EventStorage.Init();
            
            OptionButton.Select += OnSelectOption;
        }

        private void Start()
        {
            Data = GameManager.EventStorage.CurrentEvent ?? GameManager.EventStorage.GetNext();
            UpdateCard();
            
            GameManager.PlayerStats.UpdateUI();
            TaggedValue.UpdateAll("BuildVersion", GameManager.GetVersion());
        }

        /// <summary>
        /// Try to process modifiers and flags of option if it is available
        /// </summary>
        /// <returns>true - if option is available</returns>
        private bool ProcessOption()
        {
            if (!_selectedOption.IsAvailable(out List<Flag> blockedFlags))
            {
                foreach (Flag flag in blockedFlags)
                {
                    TaggedValue.AnimateAll(flag.Type, UIGenericAnimation.Animation.ButtonShake);
                }
                
                return false;
            }
            
            List<Modifier> modifiers = _selectedOption.Modifiers;
            if (modifiers != null)
            {
                foreach (Modifier modifier in modifiers)
                {
                    if (modifier.Limit == null || GameManager.PlayerStats.HasFlag(modifier.Limit))
                    {
                        GameManager.PlayerStats.SetStat(modifier.Type,
                            GameManager.PlayerStats.GetStat(modifier.Type) + modifier.Value);
                    }
                }
            }
            
            List<Flag> flags = _selectedOption.Flags;
            if (flags != null)
            {
                foreach (Flag flag in flags)
                {
                    GameManager.PlayerStats.SetFlag(flag.Type, flag.Value);
                }
            }
            
            if (GameManager.PlayerStats.HasFlag("RESTART_GAME"))
            {
                AcceptRestart();
                return false;
            }

            return true;
        }

        private void AcceptRestart()
        {
            GameManager.Restart();
            GameManager.EventStorage.Init();
            Data = GameManager.EventStorage.GetNext();
            UpdateCard();
            GameManager.PlayerStats.UpdateUI();
        }

        private void AcceptOption()
        {
            // Process long events
            if (_tldrData != null || Data.TLDR is {Count: > 0})
            {
                _tldrData ??= Data;

                if (_tldrPosition < _tldrData.TLDR.Count)
                {
                    if (!ProcessOption()) return;
                    
                    Data = _tldrData.TLDR[_tldrPosition];
                    if (Data.Title == "")
                        Data.Title = _tldrData.Title;
                    _selectedOption = null;
                    UpdateCard();
                    
                    _tldrPosition++;
                    return;
                }
            }
            _tldrData = null;
            _tldrPosition = 0;
            
            if (!ProcessOption()) return;
            
            if (!Data.SkipTurn)
                GameManager.PlayerStats.CalculateFormulas();
            
            Data = (
                    GameManager.PlayerStats.HasFlag("FAIL")
                    ? GameManager.EventStorage.GetFail()
                    : GameManager.EventStorage.GetNext()
                ) ?? GameManager.EventStorage.GetWin();

            _selectedOption = null;
            
            UpdateCard();
        }
        
        private void OnSelectOption(Option option)
        {
            if (option == null) return;

            TaggedValue.ClearPreviewAll();
            
            _selectedOption = option;
            Debug.Log($"Selected option: {option.Title}");
            AcceptOption();
        }

        private void UpdateCard()
        {
            EventDescription.text = Data.Description;
            EventDescriptionBoxDummy.text = Data.Description;
            if (EventDescriptionScroll != null && EventDescriptionScroll.verticalNormalizedPosition > 0.1f)
                EventDescriptionScroll.verticalNormalizedPosition = 0f;
            //EventTitle.text = Data.Title;
            EventPicture.sprite = ResourceLoader.GetResource<Sprite>("Icons/Events/" + Data.Category);
            if (EventPicture.sprite == null)
                EventPicture.sprite = ResourceLoader.GetResource<Sprite>("Icons/Events/Default");

            Data.Options ??= new() {new() {Title = "Далее"}};
            EventOptionsList.Options = Data.Options;

            /*AudioClip categorySound = ResourceLoader.GetResource<AudioClip>("Audio/Events/" + Data.Category);
            if (categorySound != null)
                EventSound.PlayOneShot(categorySound);*/
        }
    }
}
