<GridToolBarTemplate>
        <GridCommandButton Command="Add">Add Movie</GridCommandButton>
    </GridToolBarTemplate>
    <GridColumns>
        <GridColumn Field="Title" Title="Title" Editable="true" Width="400px" />
        <GridColumn Field="YearOfRelease" Title="Year" Editable="true" Width="100px" />
        <GridColumn Field="Genres" Title="Genres" Editable="true" Width="500px" />
        <GridCommandColumn Width="280px">
            <GridCommandButton Command="Edit" ShowInEdit="false">Edit</GridCommandButton>
            <GridCommandButton Command="Delete" ShowInEdit="false">Delete</GridCommandButton>
            <GridCommandButton Command="Save" ShowInEdit="true">Save</GridCommandButton>
            <GridCommandButton Command="Cancel" ShowInEdit="true">Cancel</GridCommandButton>
        </GridCommandColumn>
    </GridColumns>