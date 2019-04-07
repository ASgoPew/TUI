namespace TUI.Base
{
    public class GridConfiguration
    {
        public ISize[] Columns;
        public ISize[] Lines;
        public Indentation Indentation = new Indentation(UIDefault.Indentation);
        public Indentation GridIndentation = new Indentation(UIDefault.Indentation);
        public Alignment Alignment = UIDefault.Alignment;
        public Direction Direction = UIDefault.Direction;
        public Side Side = UIDefault.Side;

        public GridConfiguration(ISize[] columns = null, ISize[] lines = null)
        {
            Columns = columns ?? new ISize[] { new Relative(100) };
            Lines = lines ?? new ISize[] { new Relative(100) };
        }

        public GridConfiguration(GridConfiguration configuration)
        {
            this.Columns = (ISize[])configuration.Columns.Clone();
            this.Lines = (ISize[])configuration.Lines.Clone();
            this.Indentation = new Indentation(configuration.Indentation);
            this.Alignment = configuration.Alignment;
            this.Direction = configuration.Direction;
            this.Side = configuration.Side;
        }
    }
}
