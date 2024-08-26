namespace Admin.Data.Helpers
{
    public class StateContainerSingle<T>
    {
        private T? Item = default;

        public void SetItem(T item)
        {
            Item = item;
            NotifyChangeListeners();
        }

        public void RemoveItem() => Item = default;

        public T? GetItem() => Item;

        // TODO make some sort of object to contain the change data.
        // Especially for the normal events of cancel, save, delete etc.
        public event Action<T?>? OnChange;

        private void NotifyChangeListeners() => OnChange?.Invoke(Item);

        public void ForceNotify() => NotifyChangeListeners();
    }
}
