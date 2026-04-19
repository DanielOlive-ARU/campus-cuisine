using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CampusCuisine.Models;
using CampusCuisine.Services;

namespace CampusCuisine.ViewModel
{
  public class OrderSummaryLineViewModel : INotifyPropertyChanged
  {
    private readonly IOrderStateService? _orderState;
    private readonly IDialogService? _dialogService;

    private int _menuItemId;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private double _unitPrice;
    private int _quantity;
    private string _quantityText = "0";

    public OrderSummaryLineViewModel()
    {
      IncreaseCommand = new RelayCommand(Increase);
      DecreaseCommand = new AsyncRelayCommand(DecreaseAsync);
      RemoveCommand = new AsyncRelayCommand(RemoveAsync);
    }

    public OrderSummaryLineViewModel(OrderLineEntry source)
      : this()
    {
      UpdateFrom(source);
    }

    public OrderSummaryLineViewModel(
      OrderLineEntry source,
      IOrderStateService? orderState,
      IDialogService? dialogService)
      : this()
    {
      _orderState = orderState;
      _dialogService = dialogService;
      UpdateFrom(source);
    }

    public int MenuItemId
    {
      get => _menuItemId;
      set
      {
        if (_menuItemId != value)
        {
          _menuItemId = value;
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

    public double UnitPrice
    {
      get => _unitPrice;
      set
      {
        if (_unitPrice != value)
        {
          _unitPrice = value;
          OnPropertyChanged();
          OnPropertyChanged(nameof(LineTotal));
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
          _quantityText = _quantity.ToString();
          OnPropertyChanged();
          OnPropertyChanged(nameof(QuantityText));
          OnPropertyChanged(nameof(LineTotal));
        }
      }
    }

    public string QuantityText
    {
      get => _quantityText;
      set
      {
        if (_quantityText != value)
        {
          _quantityText = value;
          OnPropertyChanged();
        }
      }
    }

    public double LineTotal => _unitPrice * _quantity;

    public ICommand IncreaseCommand { get; }

    public ICommand DecreaseCommand { get; }

    public ICommand RemoveCommand { get; }

    public void UpdateFrom(OrderLineEntry source)
    {
      MenuItemId = source.MenuItemId;
      Name = source.Name;
      Description = source.Description;
      UnitPrice = source.UnitPrice;
      Quantity = source.Quantity;
    }

    public static bool TryValidateQuantity(string? text, out int validated, out string? errorMessage)
    {
      validated = 0;
      errorMessage = null;

      var trimmed = text?.Trim();
      if (string.IsNullOrWhiteSpace(trimmed) || !int.TryParse(trimmed, out var parsed))
      {
        errorMessage = "Please enter a whole number quantity.";
        return false;
      }

      if (parsed <= 0)
      {
        errorMessage = "Quantity must be greater than zero.";
        return false;
      }

      if (parsed > 999)
      {
        errorMessage = "Quantity is too large.";
        return false;
      }

      validated = parsed;
      return true;
    }

    private void Increase()
    {
      if (_orderState is null) return;
      _orderState.SetQuantity(MenuItemId, Quantity + 1);
    }

    private async Task DecreaseAsync()
    {
      if (_orderState is null) return;

      if (Quantity > 1)
      {
        _orderState.SetQuantity(MenuItemId, Quantity - 1);
        return;
      }

      if (_dialogService is null)
      {
        _orderState.RemoveLine(MenuItemId, Quantity);
        return;
      }

      var confirm = await _dialogService.ConfirmAsync(
        "Remove Item",
        $"Remove '{Name}' from your order?",
        "Remove",
        "Cancel");

      if (confirm)
        _orderState.RemoveLine(MenuItemId, Quantity);
    }

    private async Task RemoveAsync()
    {
      if (_orderState is null) return;

      if (_dialogService is null)
      {
        _orderState.RemoveLine(MenuItemId, Quantity);
        return;
      }

      var confirm = await _dialogService.ConfirmAsync(
        "Remove Item",
        $"Remove '{Name}' from your order?",
        "Remove",
        "Cancel");

      if (confirm)
        _orderState.RemoveLine(MenuItemId, Quantity);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
