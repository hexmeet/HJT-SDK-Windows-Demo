using EasyVideoWin.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EasyVideoWin.ViewModel
{
    abstract class AudioDeviceModel : ViewModelBase
    {
        protected abstract string GetCurrentDevice();
        protected abstract ObservableCollection<string> GetDevices();
        protected abstract void SelectDevice(string device);

        private List<AudioDeviceItem> _itemList = null;
        private int _pageIndex = 0;
        private int _pageCount = 0;
        private const int PAGE_SIZE = 3;
        private int _selectedIndex = 0;
        public ObservableCollection<AudioDeviceItem> CurrentGroup { get; set; }
        public ObservableCollection<PageDot> PageDots { get; set; }

        public RelayCommand PageLeftCommand { get; set; }
        public RelayCommand PageRightCommand { get; set; }

        private Visibility _pageBtnVisibility = Visibility.Hidden;
        public Visibility PageBtnVisibility
        {
            get
            {
                return _pageBtnVisibility;
            }

            set
            {
                _pageBtnVisibility = value;
                OnPropertyChanged("PageBtnVisibility");
            }
        }

        private bool _pageLeftEnabled = false;
        public bool PageLeftEnabled
        {
            get
            {
                return _pageLeftEnabled;
            }

            set
            {
                _pageLeftEnabled = value;
                OnPropertyChanged("PageLeftEnabled");
            }
        }

        private bool _pageRightEnabled = false;
        public bool PageRightEnabled
        {
            get
            {
                return _pageRightEnabled;
            }

            set
            {
                _pageRightEnabled = value;
                OnPropertyChanged("PageRightEnabled");
            }
        }

        public AudioDeviceModel()
        {
            PageLeftCommand = new RelayCommand(PageLeft);
            PageRightCommand = new RelayCommand(PageRight);

            CurrentGroup = new ObservableCollection<AudioDeviceItem>();
            PageDots = new ObservableCollection<PageDot>();

            string currentDevice = GetCurrentDevice();
            _itemList = new List<AudioDeviceItem>();
            int index = 0;
            foreach(string device in GetDevices())
            {
                if (!string.IsNullOrEmpty(device))
                {
                    AudioDeviceItem deviceItem = new AudioDeviceItem();
                    deviceItem.Name = device;
                    deviceItem.Desc = "";
                    deviceItem.OriginalName = device;
                    if (device.Equals(currentDevice))
                    {
                        deviceItem.Selected = true;
                        _selectedIndex = index;
                    }
                    else
                    {
                        deviceItem.Selected = false;
                    }
                    _itemList.Add(deviceItem);
                    index++;
                }
            }

            if (_itemList != null && _itemList.Count > 0)
            {
                _pageCount = _itemList.Count / PAGE_SIZE + (_itemList.Count % PAGE_SIZE == 0 ? 0 : 1);
                if (_pageCount == 1)
                {
                    PageBtnVisibility = Visibility.Hidden;
                }
                else
                {
                    PageBtnVisibility = Visibility.Visible;
                    for (int i = 0; i < _pageCount; i++)
                    {
                        PageDot dot = new PageDot();
                        PageDots.Add(dot);
                    }
                }
                RefreshItems();
            }
        }

        private void RefreshCurrentGroup()
        {
            CurrentGroup.Clear();
            int startIndex = _pageIndex * PAGE_SIZE;
            for (int i = 0; i < PAGE_SIZE; i++)
            {
                if (startIndex + i < _itemList.Count)
                {
                    CurrentGroup.Add(_itemList[startIndex + i]);
                    if (startIndex + i == _selectedIndex)
                    {
                        _itemList[startIndex + i].Selected = true;
                    }
                }
            }
        }

        private void RefreshPageButton()
        {
            if (_pageCount > 1)
            {
                if (_pageIndex == 0)
                {
                    PageLeftEnabled = false;
                }
                else
                {
                    PageLeftEnabled = true;
                }

                if (_pageIndex == _pageCount - 1)
                {
                    PageRightEnabled = false;
                }
                else
                {
                    PageRightEnabled = true;
                }
            }
        }

        private void RefreshPageDots()
        {
            if (PageDots.Count <= 1)
            {
                return;
            }

            for (int i = 0; i < PageDots.Count; i++)
            {
                if (i == _pageIndex)
                {
                    PageDots[i].Active = true;
                }
                else
                {
                    PageDots[i].Active = false;
                }
            }
        }

        public void SelectItem(int index)
        {
            int targetIndex = _pageIndex * PAGE_SIZE + index;
            if (_selectedIndex == targetIndex || targetIndex >= _itemList.Count)
            {
                return;
            }
            SelectDevice(_itemList[targetIndex].OriginalName);
            foreach (AudioDeviceItem item in _itemList)
            {
                item.Selected = false;
            }
            CurrentGroup[index].Selected = true;
            _selectedIndex = targetIndex;
        }

        private void RefreshItems()
        {
            RefreshCurrentGroup();
            RefreshPageButton();
            RefreshPageDots();
        }

        private void PageLeft(object parameter)
        {
            _pageIndex--;
            RefreshItems();
        }

        private void PageRight(object parameter)
        {
            _pageIndex++;
            RefreshItems();
        }
    }

    class AudioDeviceItem : ViewModelBase
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string OriginalName { get; set; }

        private bool _selected = false;
        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                OnPropertyChanged("SelectedIconVisibility");
            }
        }

        public Visibility SelectedIconVisibility
        {
            get
            {
                return Selected ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private bool _placeHolder;
        public bool PlaceHolder
        {
            get
            {
                return _placeHolder;
            }
            set
            {
                _placeHolder = value;
                OnPropertyChanged("ItemVisibility");
            }
        }

        public Visibility ItemVisibility
        {
            get
            {
                return PlaceHolder ? Visibility.Hidden : Visibility.Visible;
            }
        }
    }

    class PageDot : ViewModelBase
    {
        private bool _active;
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                _active = value;
                OnPropertyChanged("FillColor");
            }
        }
        public string FillColor
        {
            get
            {
                return Active ? "#c1c1c1" : "#f1f1f1";
            }
        }
    }
}
