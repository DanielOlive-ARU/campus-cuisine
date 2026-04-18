using System.Collections.ObjectModel;
using System.ComponentModel;
using CampusCuisine.Models;

namespace CampusCuisine.Services;

public interface IOrderStateService : INotifyPropertyChanged
{
  ObservableCollection<OrderLineDto> Lines { get; }

  int TotalItems { get; }

  double GrandTotal { get; }

  bool HasOrder { get; }

  void AddLine(int menuItemId, string? name = null, double unitPrice = 0, int quantity = 1, string? description = null);

  void RemoveLine(int menuItemId, int quantity = 1);

  void SetQuantity(int menuItemId, int quantity);

  void Clear();

  int GetQuantityForMenuItem(int menuItemId);

  CreateOrderRequestDto ToCreateOrderRequest();
}
