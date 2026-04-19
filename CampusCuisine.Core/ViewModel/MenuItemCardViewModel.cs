using System.ComponentModel;
using System.Runtime.CompilerServices;
using CampusCuisine.Models;

namespace CampusCuisine.ViewModel
{
  public class MenuItemCardViewModel : INotifyPropertyChanged
  {
    private int _id;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private decimal _price;
    private string _imageUrl = string.Empty;
    private int _quantity;

    public MenuItemCardViewModel()
    {
    }

    public MenuItemCardViewModel(MenuItemModel source, int quantity = 0)
    {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      Id = source.Id;
      Name = source.Name;
      Description = source.Description;
      Price = source.Price;
      ImageUrl = source.ImageUrl;
      Quantity = quantity;
    }

    public int Id
    {
      get => _id;
      set
      {
        if (_id != value)
        {
          _id = value;
          OnPropertyChanged();
        }
      }
    }

    public string Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value;
          OnPropertyChanged();
        }
      }
    }

    public string Description
    {
      get => _description;
      set
      {
        if (_description != value)
        {
          _description = value;
          OnPropertyChanged();
        }
      }
    }

    public decimal Price
    {
      get => _price;
      set
      {
        if (_price != value)
        {
          _price = value;
          OnPropertyChanged();
        }
      }
    }

    public string ImageUrl
    {
      get => _imageUrl;
      set
      {
        if (_imageUrl != value)
        {
          _imageUrl = value;
          OnPropertyChanged();
        }
      }
    }

    public int Quantity
    {
      get => _quantity;
      set
      {
        if (_quantity != value)
        {
          _quantity = value;
          OnPropertyChanged();
          OnPropertyChanged(nameof(HasQuantity));
          OnPropertyChanged(nameof(QuantityText));
        }
      }
    }

    public bool HasQuantity => _quantity > 0;

    public string QuantityText => $"In order: {_quantity}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
