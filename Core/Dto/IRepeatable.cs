namespace Core.Dto
{
    interface IRepeatable
    {
        public RepeaterTypeDto? RepeatType { get; }

        public ICollection<RepeaterDto> Repeats { get; }
    }
}
