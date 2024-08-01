namespace Admin.Data.Helpers
{
    public class StateContainerSingle<T>
    {
        private T? Item = default;

        public void SetItem(T item) => Item = item;

        public void RemoveItem() => Item = default;

        public T? GetItem() => Item;

        // TODO make some sort of object to contain the change data.
        // Especially for the normal events of cancel, save, delete etc.
        public event Action? OnChange;

        public void NotifyChangeListeners() => OnChange?.Invoke();
    }
}
