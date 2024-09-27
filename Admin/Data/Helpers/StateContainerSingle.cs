namespace Admin.Data.Helpers
{
    public class StateContainerSingle<T>
    {
        private T? _item = default;

        public void SetItem(T item)
        {
            _item = item;
            NotifyChangeListeners();
        }

        public void RemoveItem() => _item = default;

        public T? GetItem() => _item;

        // TODO make some sort of object to contain the change data.
        // Especially for the normal events of cancel, save, delete etc.
        public event Action<T?>? OnChange;

        private void NotifyChangeListeners() => OnChange?.Invoke(_item);

        public void ForceNotify() => NotifyChangeListeners();
    }
}
