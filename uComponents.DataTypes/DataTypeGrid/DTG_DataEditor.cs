﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
// 11.08.2011 - Created [Ove Andersen]
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using uComponents.Core;
using uComponents.DataTypes.DataTypeGrid.Model;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

[assembly: WebResource("uComponents.DataTypes.DataTypeGrid.Css.DTG_DataEditor.css", Constants.MediaTypeNames.Text.Css, PerformSubstitution = true)]
[assembly: WebResource("uComponents.DataTypes.DataTypeGrid.Scripts.jquery.dataTables.min.js", Constants.MediaTypeNames.Application.JavaScript)]
[assembly: WebResource("uComponents.DataTypes.DataTypeGrid.Scripts.DTG_DataEditor.js", Constants.MediaTypeNames.Application.JavaScript)]

namespace uComponents.DataTypes.DataTypeGrid
{
	using System.Web;

	using uComponents.DataTypes.DataTypeGrid.Extensions;
	using uComponents.DataTypes.DataTypeGrid.Functions;
	using umbraco;


	/// <summary>
	/// The DataType Grid Control
	/// </summary>
	[ClientDependency.Core.ClientDependency(ClientDependency.Core.ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
	public class DataEditor : Control, INamingContainer, IDataEditor
	{
		#region Public

		/// <summary>
		/// Gets or sets the configuration.
		/// </summary>
		public List<StoredValueRow> Rows { get; set; }

		/// <summary>
		/// Gets or sets the grid.
		/// </summary>
		public Table Grid { get; set; }

		/// <summary>
		/// Gets or sets the grid.
		/// </summary>
		public Panel Toolbar { get; set; }

		/// <summary>
		/// Gets or sets the insert controls.
		/// </summary>
		public Panel InsertControls { get; set; }

		/// <summary>
		/// Gets or sets the edit controls.
		/// </summary>
		public Panel EditControls { get; set; }

		/// <summary>
		/// Gets or sets the delete controls.
		/// </summary>
		public Panel DeleteControls { get; set; }

		/// <summary>
		/// Gets or sets the current row.
		/// </summary>
		/// <value>The current row.</value>
		public int CurrentRow
		{
			get
			{
				if (ViewState["CurrentRow"] != null)
				{
					return (int) ViewState["CurrentRow"];
				}

				return 0;
			}

			set
			{
				ViewState["CurrentRow"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data string.
		/// </summary>
		/// <value>The data string.</value>
		public string DataString
		{
			get
			{
				if (ViewState["DataString"] != null)
				{
					DtgHelpers.AddLogEntry(string.Format("DTG: Returned value from ViewState: {0}", ViewState["DataString"]));
					
					return ViewState["DataString"].ToString();
				}

				DtgHelpers.AddLogEntry(string.Format("DTG: ViewState did not contain data."));

				return string.Empty;
			}

			set
			{
				DtgHelpers.AddLogEntry(string.Format("DTG: Stored the following data in ViewState: {0}", value));

				ViewState["DataString"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the show table header.
		/// </summary>
		/// <value>
		/// The show table header.
		/// </value>
		public HiddenField ShowTableHeader { get; set; }

		/// <summary>
		/// Gets or sets the show table footer.
		/// </summary>
		/// <value>
		/// The show table footer.
		/// </value>
		public HiddenField ShowTableFooter { get; set; }

		/// <summary>
		/// Gets or sets the number of rows.
		/// </summary>
		/// <value>The number of rows.</value>
		public HiddenField NumberOfRows { get; set; }

		/// <summary>
		/// Gets or sets the content sorting.
		/// </summary>
		/// <value>
		/// The content sorting.
		/// </value>
		public HiddenField ContentSorting { get; set; }

		/// <summary>
		/// Gets or sets the stored prevalues.
		/// </summary>
		/// <value>The stored pre values.</value>
		public List<PreValueRow> StoredPreValues { get; set; }

		/// <summary>
		/// Gets er sets the insert data types
		/// </summary>
		/// <value>The insert data types.</value>
		public List<StoredValue> InsertDataTypes { get; set; }

		/// <summary>
		/// Gets or sets the edit data types.
		/// </summary>
		/// <value>The edit data types.</value>
		public List<StoredValue> EditDataTypes { get; set; }

		public override string ID
		{
			get
			{
				return this.id ?? (this.id = "DTG_" + this.dataTypeDefinitionId + "_" + this.instanceId);
			}
		}

		#endregion

		#region Private

		/// <summary>
		/// The control id
		/// </summary>
		private string id;

		/// <summary>
		/// The unique instance id
		/// </summary>
		private readonly string instanceId;

		/// <summary>
		/// Value stored by a datatype instance
		/// </summary>
		private readonly IData data;

		/// <summary>
		/// The datatype definition id
		/// </summary>
		private readonly int dataTypeDefinitionId;

		/// <summary>
		/// The settings.
		/// </summary>
		private readonly PreValueEditorSettings settings;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="DataEditor"/> class.
		/// </summary>
		/// <param name="data">
		/// The data.
		/// </param>
		/// <param name="settings">
		/// The settings.
		/// </param>
		/// <param name="dataTypeDefinitionId">
		/// The unique id.
		/// </param>
		public DataEditor(IData data, PreValueEditorSettings settings, int dataTypeDefinitionId, string instanceId)
		{
			this.settings = settings;
			this.data = data;
			this.dataTypeDefinitionId = dataTypeDefinitionId;
		    this.instanceId = instanceId;
		}

		#region IDataEditor Members

		/// <summary>
		/// Saves this instance.
		/// </summary>
		public void Save()
		{
			this.data.Value = string.IsNullOrEmpty(DataString) ? this.data.Value : DataString;

			// Get new values
			Rows = GetStoredValues();

			// Refresh grid
			RefreshGrid();

			// Clear input controls
			ClearControls();

			DtgHelpers.AddLogEntry(string.Format("DTG: Saved the following data to database: {0}", this.data.Value));
		}

		/// <summary>
		/// Stores this instance temporarily.
		/// </summary>
		public void Store()
		{
			// Start data
			var str = "<items>";

			foreach (var container in Rows)
			{
				// Start
				str += string.Concat(
					"<item id='", container.Id.ToString(), "'>");

				foreach (var v in container.Cells)
				{
					if (v.Value.Data.Value == null)
					{
						v.Value.Data.Value = string.Empty;
					}

					str += string.Concat(
						"<", 
						v.Alias, 
						" nodeName='", 
						v.Name, 
						"' nodeType='", 
						v.Value.DataTypeDefinitionId,
						"'>",
						HttpUtility.HtmlEncode(v.Value.Data.Value.ToString()), 
						"</", 
						v.Alias, 
						">");
				}

				// End row
				str += "</item>";
			}

			// End data
			str += "</items>";

			// Save values
			DataString = str;

			// Refresh grid
			RefreshGrid();

			// Clear input controls
			ClearControls();
		}

		/// <summary>
		/// Gets a value indicating whether [show label].
		/// </summary>
		/// <value>
		///   <c>true</c> if [show label]; otherwise, <c>false</c>.
		/// </value>
		public virtual bool ShowLabel
		{
			get { return this.settings.ShowLabel; }
		}

		/// <summary>
		/// Gets a value indicating whether [treat as rich text editor].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
		/// </value>
		public virtual bool TreatAsRichTextEditor
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the editor.
		/// </summary>
		public Control Editor
		{
			get { return this; }
		}

		#endregion

		#region Custom

		/// <summary>
		/// Refreshes the grid.
		/// </summary>
		private void RefreshGrid()
		{
			// Remove all rows
			Grid.Rows.Clear();

			// Re-add rows
			GenerateHeaderRow();
			GenerateValueRows();
		}

		/// <summary>
		/// The clear controls.
		/// </summary>
		private void ClearControls()
		{
			InsertDataTypes = GetInsertDataTypes();
			GenerateInsertControls();

			CurrentRow = 0;

			EditDataTypes = GetEditDataTypes();
			GenerateEditControls();
		}

		/// <summary>
		/// Generates the header row.
		/// </summary>
		private void GenerateHeaderRow()
		{
			var tr = new TableRow { TableSection = TableRowSection.TableHeader };

			// Add ID header cell
			tr.Cells.Add(new TableHeaderCell { Text = uQuery.GetDictionaryItem("ID", "ID") });

			// NOTE: Temporary
			tr.Cells.Add(new TableHeaderCell { Text = uQuery.GetDictionaryItem("Actions", "Actions") });

			// Add prevalue cells
			foreach (var s in StoredPreValues)
			{
				var th = new TableHeaderCell {Text = s.Name};
				tr.Cells.Add(th);
			}

			Grid.Rows.Add(tr);
		}

		/// <summary>
		/// Generates the value rows.
		/// </summary>
		private void GenerateValueRows()
		{
			foreach (var row in Rows)
			{
				var tr = new TableRow();

				// Add ID column
				var id = new TableCell();
				id.Controls.Add(new Label { Text = row.Id.ToString() });

				tr.Cells.Add(id);

				// NOTE: Temporary
				var actions = new TableCell();

				var dInner = new HtmlGenericControl("span");
				dInner.Attributes["class"] = "ui-button-text";
				dInner.InnerText = uQuery.GetDictionaryItem("Delete", "Delete");

				var dIcon = new HtmlGenericControl("span");
				dIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-close";

				var deleteRow = new LinkButton
									{
										ID = "DeleteButton_" + row.Id,
										CssClass = "deleteRowDialog ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only", 
										CommandArgument = row.Id.ToString(), 
										OnClientClick = "return confirm('Are you sure you want to delete this?')"
									};
				deleteRow.Click += deleteRow_Click;

				deleteRow.Controls.Add(dIcon);
				deleteRow.Controls.Add(dInner);

				var eInner = new HtmlGenericControl("span");
				eInner.Attributes["class"] = "ui-button-text";
				eInner.InnerText = uQuery.GetDictionaryItem("Edit", "Edit");

				var eIcon = new HtmlGenericControl("span");
				eIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-pencil";

				var editRow = new LinkButton
								  {
									  ID = "EditButton_" + row.Id,
									  CssClass = "editRowDialog ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only", 
									  CommandArgument = row.Id.ToString()
								  };
				editRow.Click += editRow_Click;

				editRow.Controls.Add(eIcon);
				editRow.Controls.Add(eInner);

				// NOTE: See here for reference
				// http://www.deviantpoint.com/post/2009/03/12/Using-jQuery-UI-Dialogs-for-confirmation-windows.aspx
				// http://www.codedigest.com/Articles/ASPNET/314_Multiple_Ways_to_Call_Javascript_Function_from_CodeBehind_in_ASPNet.aspx

				actions.Controls.Add(deleteRow);
				actions.Controls.Add(editRow);

				tr.Cells.Add(actions);

				// NOTE: End Temporary

				// Print stored values
				foreach (var storedConfig in StoredPreValues)
				{
					var td = new TableCell();

					foreach (var value in row.Cells)
					{
						var text = new Label { Text = value.Value.ToDtgString() };

						if (value.Name.Equals(storedConfig.Name))
						{
							td.Controls.Add(text);
						}
					}

					tr.Cells.Add(td);
				}

				Grid.Rows.Add(tr);
			}
		}

		/// <summary>
		/// Generates the footer row.
		/// </summary>
		private void GenerateFooterToolbar()
		{
			var inner = new HtmlGenericControl("span") { InnerText = uQuery.GetDictionaryItem("Add", "Add") };
			inner.Attributes["class"] = "ui-button-text";

			var icon = new HtmlGenericControl("span");
			icon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-plus";

			var addRowDialog = new LinkButton
								   {
									   ID = "InsertRowDialog",
									   CssClass = "insertRowDialog ui-button ui-widget ui-state-default ui-corner-all ui-button-text-icon-primary"
								   };
			addRowDialog.Click += this.addRowDialog_Click;

			addRowDialog.Controls.Add(icon);
			addRowDialog.Controls.Add(inner);

			Toolbar.Controls.Add(addRowDialog);
		}

		/// <summary>
		/// Generates the validation controls.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="config">The config.</param>
		/// <param name="list">The list.</param>
		private void GenerateValidationControls(Control parent, string name, StoredValue config, IList<StoredValue> list)
		{
			var control = parent.FindControl(config.Value.DataEditor.Editor.ID);

			if (!string.IsNullOrEmpty(StoredPreValues.Single(x => x.Alias == config.Alias).ValidationExpression) && control != null)
			{
				try
				{
					var regex = new Regex(@StoredPreValues.Single(x => x.Alias == config.Alias).ValidationExpression);
					var validator = new RegularExpressionValidator()
					{
						ID = name + config.Alias + "_" + list.IndexOf(config),
						Enabled = false,
						CssClass = "validator",
						ControlToValidate = control.ID,
						Display = ValidatorDisplay.Dynamic,
						ValidationExpression = regex.ToString(),
						ErrorMessage = config.Name + " is not in a correct format"
					};
					parent.Controls.Add(validator);
				}
				catch (ArgumentException ex)
				{
					parent.Controls.Add(new HtmlGenericControl("span")
					{
						InnerText = string.Concat("Regex validation expression is invalid. Validation will not occur.", "<!-- ", ex, " -->")
					});
				}
			}
		}

		/// <summary>
		/// Generates the insert controls.
		/// </summary>
		private void GenerateInsertControls()
		{
			InsertControls.Controls.Clear();

			InsertControls.Controls.Add(new LiteralControl("<ul>"));

			foreach (var config in InsertDataTypes)
			{
				var control = config.Value.DataEditor.Editor;
				control.ID = "Insert" + config.Alias;

				// Configure the datatype so it works with DTG
				config.Value.ConfigureForDtg(InsertControls);

				InsertControls.Controls.Add(new LiteralControl("<li>"));
				InsertControls.Controls.Add(new Label { CssClass = "insertControlLabel", Text = config.Name });
				InsertControls.Controls.Add(control);
				GenerateValidationControls(InsertControls, "Insert", config, InsertDataTypes);

				InsertControls.Controls.Add(new LiteralControl("</li>"));
			}

			InsertControls.Controls.Add(new LiteralControl("</ul>"));

			var iInner = new HtmlGenericControl("span") { InnerText = uQuery.GetDictionaryItem("Add", "Add") };
			iInner.Attributes["class"] = "ui-button-text";

			var iIcon = new HtmlGenericControl("span");
			iIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-plus";

			var addRow = new LinkButton
							 {
								 ID = "InsertButton",
								 CssClass = "insertButton ui-button ui-widget ui-state-default ui-corner-all ui-button-text-icon-primary"
							 };           
			addRow.Click += addRow_Click;

			addRow.Controls.Add(iIcon);
			addRow.Controls.Add(iInner);

			InsertControls.Controls.Add(addRow);
		}

		/// <summary>
		/// Handles the Click event of the addRow control.
		/// </summary>
		/// <param name="sender">
		/// The source of the event.
		/// </param>
		/// <param name="e">
		/// The <see cref="System.EventArgs"/> instance containing the event data.
		/// </param>
		protected void addRow_Click(object sender, EventArgs e)
		{
			var row = new StoredValueRow { Id = GetAvailableId() };

			foreach (var t in InsertDataTypes)
			{
				// Save value to datatype
				t.Value.SaveForDtg();

				// Create new storedvalue object
				var v = new StoredValue
							{
								Name = t.Name, 
								Alias = t.Alias, 
								Value = t.Value
							};

				row.Cells.Add(v);
			}

			Rows.Add(row);

			Store();
			Save();
		}

		/// <summary>
		/// Generates the edit controls.
		/// </summary>
		private void GenerateEditControls()
		{
			EditControls.Controls.Clear();

			EditControls.Controls.Add(new LiteralControl("<ul>"));

			foreach (var config in EditDataTypes)
			{
				var control = config.Value.DataEditor.Editor;
				control.ID = "Edit" + config.Alias;

				// Configure the datatype so it works with DTG
				config.Value.ConfigureForDtg(EditControls);

				EditControls.Controls.Add(new LiteralControl("<li>"));
				EditControls.Controls.Add(new Label { CssClass = "editControlLabel", Text = config.Name });
				EditControls.Controls.Add(control);
				GenerateValidationControls(EditControls, "Edit", config, EditDataTypes);

				EditControls.Controls.Add(new LiteralControl("</li>"));
			}

			EditControls.Controls.Add(new LiteralControl("</ul>"));

			var uInner = new HtmlGenericControl("span") { InnerText = uQuery.GetDictionaryItem("Update", "Update") };
			uInner.Attributes["class"] = "ui-button-text";

			var uIcon = new HtmlGenericControl("span");
			uIcon.Attributes["class"] = "ui-button-icon-primary ui-icon ui-icon-pencil";

			var updateRow = new LinkButton
								{
									ID = "UpdateButton",
									CssClass = "updateButton ui-button ui-widget ui-state-default ui-corner-all ui-button-text-icon-primary"
								};
			updateRow.Click += updateRow_Click;

			updateRow.Controls.Add(uIcon);
			updateRow.Controls.Add(uInner);

			EditControls.Controls.Add(updateRow);
		}

		/// <summary>
		/// Handles the Click event of the editRow control.
		/// </summary>
		/// <param name="sender">
		/// The source of the event.
		/// </param>
		/// <param name="e">
		/// The <see cref="System.EventArgs"/> instance containing the event data.
		/// </param>
		protected void editRow_Click(object sender, EventArgs e)
		{
			CurrentRow = int.Parse(((LinkButton) sender).CommandArgument);

			EditDataTypes = GetEditDataTypes();
			GenerateEditControls();

			ScriptManager.RegisterClientScriptBlock(this, GetType(), "OpenEditDialog_" + this.ID, "openDialog('" + this.ClientID + "_ctrlEdit')", true);
		}

		/// <summary>
		/// Handles the Click event of the addRowDialog control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void addRowDialog_Click(object sender, EventArgs e)
		{
			ClearControls();

			ScriptManager.RegisterClientScriptBlock(this, GetType(), "OpenInsertDialog_" + this.ID, "openDialog('" + this.ClientID + "_ctrlInsert')", true);
		}

		/// <summary>
		/// Handles the Click event of the updateRow control.
		/// </summary>
		/// <param name="sender">
		/// The source of the event.
		/// </param>
		/// <param name="e">
		/// The <see cref="System.EventArgs"/> instance containing the event data.
		/// </param>
		protected void updateRow_Click(object sender, EventArgs e)
		{
			foreach (var row in Rows.Where(row => row.Id == CurrentRow))
			{
				foreach (var cell in row.Cells)
				{
					// Save value to datatype
					cell.Value.SaveForDtg();
				}
			}

			Store();
			Save();
		}

		/// <summary>
		/// Generates the delete controls.
		/// </summary>
		/// <param name="rowId">
		/// The row Id.
		/// </param>
		private void GenerateDeleteControls(Guid rowId)
		{
		}

		/// <summary>
		/// Handles the Click event of the deleteRow control.
		/// </summary>
		/// <param name="sender">
		/// The source of the event.
		/// </param>
		/// <param name="e">
		/// The <see cref="System.EventArgs"/> instance containing the event data.
		/// </param>
		protected void deleteRow_Click(object sender, EventArgs e)
		{
			var rowToDelete = new StoredValueRow();
			foreach (
				StoredValueRow row in Rows.Where(row => row.Id.ToString().Equals(((LinkButton) sender).CommandArgument))
				)
			{
				rowToDelete = row;
			}

			Rows.Remove(rowToDelete);

			Store();
			Save();
		}

		/// <summary>
		/// Gets the stored values.
		/// </summary>
		/// <returns></returns>
		private List<StoredValueRow> GetStoredValues()
		{
			var values = new List<StoredValueRow>();

			// Add root element if value is empty
			if (string.IsNullOrEmpty(this.data.Value.ToString()))
			{
				this.data.Value = "<items></items>";
			}

			var doc = new XmlDocument();
			doc.LoadXml(this.data.Value.ToString());

			// Create and add XML declaration. 
			var xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
			var root = doc.DocumentElement;
			doc.InsertBefore(xmldecl, root);

			// Get stored values from database
			if (root.ChildNodes.Count > 0)
			{
				foreach (XmlNode container in root.ChildNodes)
				{
					// <DataTypeGrid>
					var valueRow = new StoredValueRow();
					if (container.Attributes != null)
					{
						valueRow.Id = int.Parse(container.Attributes["id"].Value);
					}

					foreach (PreValueRow config in StoredPreValues)
					{
						var value = new StoredValue
										{
											Name = config.Name, 
											Alias = config.Alias
										};

						var datatypeid = config.DataTypeId;

						if (datatypeid != 0)
						{
							var dtd = DataTypeDefinition.GetDataTypeDefinition(datatypeid);
							var dt = dtd.DataType;
							dt.Data.Value = string.Empty;
							value.Value = dt;

							foreach (XmlNode node in container.ChildNodes)
							{
								if (config.Alias.Equals(node.Name))
								{
									value.Value.Data.Value = node.InnerText;
								}
							}

							valueRow.Cells.Add(value);
						}
					}

					values.Add(valueRow);
				}
			}

			// Set the configuration
			return values;
		}

		/// <summary>
		/// Gets the insert data types.
		/// </summary>
		/// <returns>
		/// </returns>
		private List<StoredValue> GetInsertDataTypes()
		{
			var list = new List<StoredValue>();

			foreach (var config in StoredPreValues)
			{
				var dtd = DataTypeDefinition.GetDataTypeDefinition(config.DataTypeId);
				var dt = dtd.DataType;

				var s = new StoredValue
							{
								Name = config.Name, 
								Alias = config.Alias, 
								Value = dt
							};

				list.Add(s);
			}

			return list;
		}

		/// <summary>
		/// The get edit data types.
		/// </summary>
		/// <returns>
		/// </returns>
		private List<StoredValue> GetEditDataTypes()
		{
			var list = new List<StoredValue>();

			if (CurrentRow > 0)
			{
				list = GetStoredValueRow(CurrentRow).Cells;
			}
			else
			{
				foreach (var config in StoredPreValues)
				{
					var dtd = DataTypeDefinition.GetDataTypeDefinition(config.DataTypeId);
					var dt = dtd.DataType;

					var s = new StoredValue
								{
									Name = config.Name, 
									Alias = config.Alias, 
									Value = dt
								};

					list.Add(s);
				}
			}

			return list;
		}

		/// <summary>
		/// The get stored value row.
		/// </summary>
		/// <param name="id">
		/// The id.
		/// </param>
		/// <returns>
		/// </returns>
		private StoredValueRow GetStoredValueRow(int id)
		{
			foreach (var row in this.Rows.Where(row => row.Id == id))
			{
				return row;
			}

			return new StoredValueRow();
		}

		/// <summary>
		/// Gets an available id.
		/// </summary>
		/// <returns>
		/// The get available id.
		/// </returns>
		public int GetAvailableId()
		{
			var newId = 1;

			foreach (StoredValueRow row in Rows)
			{
				if (newId <= row.Id)
				{
					newId = row.Id + 1;
				}
			}

			return newId;
		}

		#endregion

		/// <summary>
		/// Initialize the control, make sure children are created
		/// </summary>
		/// <param name="e">
		/// An <see cref="T:System.EventArgs"/> object that contains the event data.
		/// </param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			// Adds the client dependencies
			this.AddAllDtgClientDependencies();
		}

		/// <summary>
		/// Add the resources (styles/scripts)
		/// </summary>
		/// <param name="e">
		/// The <see cref="T:System.EventArgs"/> object that contains the event data.
		/// </param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

		    this.id = "DTG_" + this.dataTypeDefinitionId + "_" + this.instanceId;
		}

		/// <summary>
		/// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			EnsureChildControls();

			// DEBUG: Reset stored values
			// this.Data.Value = "<items><item id='1'><name nodeName='Name' nodeType='-88' >Anna</name><age nodeName='Age' nodeType='-51' >25</age><picture nodeName='Picture' nodeType='1035' ></picture></item><item id='6'><name nodeName='Name' nodeType='-88' >Ove</name><gender nodeName='Gender' nodeType='-88'>Male</gender><age nodeName='Age' nodeType='-51' >23</age><picture nodeName='Picture' nodeType='1035' ></picture></item></items>";

			// Set default value if none exists
			if (this.data.Value == null)
			{
				DtgHelpers.AddLogEntry(string.Format("DTG: No values exist in database for this property"));
				
				this.data.Value = string.Empty;
			}
			else
			{
				DtgHelpers.AddLogEntry(string.Format("DTG: Retrieved the following data from database: {0}", this.data.Value));
			}

			ShowTableHeader = new HiddenField() { ID = "ShowTableHeader", Value = this.settings.ShowTableHeader.ToString() };
			ShowTableFooter = new HiddenField() { ID = "ShowTableFooter", Value = this.settings.ShowTableFooter.ToString() };
			NumberOfRows = new HiddenField() { ID ="NumberOfRows", Value = this.settings.NumberOfRows.ToString() };
			ContentSorting = new HiddenField() { ID = "ContentSorting", Value = this.settings.ContentSorting };
			Grid = new Table { ID = "tblGrid", CssClass = "display" };
			Toolbar = new Panel { ID = "pnlToolbar", CssClass = "Toolbar" };

			StoredPreValues = DtgHelpers.GetConfig(this.dataTypeDefinitionId);
			Rows = GetStoredValues();
			InsertDataTypes = GetInsertDataTypes();
			EditDataTypes = GetEditDataTypes();

			InsertControls = new Panel { ID = "ctrlInsert", CssClass = "InsertControls" };
			EditControls = new Panel { ID = "ctrlEdit", CssClass = "EditControls" };
			DeleteControls = new Panel { ID = "ctrlDelete", CssClass = "DeleteControls" };

			// Generate header row
			GenerateHeaderRow();

			// Generate rows with edit, delete and row data
			GenerateValueRows();

			// Generate header row
			GenerateFooterToolbar();

			// Generate insert controls
			GenerateInsertControls();

			// Generate edit controls
			GenerateEditControls();

			this.Controls.Add(this.ShowTableHeader);
			this.Controls.Add(this.ShowTableFooter);
			this.Controls.Add(this.NumberOfRows);
			this.Controls.Add(this.ContentSorting);
			this.Controls.Add(this.Grid);
			this.Controls.Add(this.Toolbar);
			this.Controls.Add(this.InsertControls);
			this.Controls.Add(this.EditControls);
			this.Controls.Add(this.DeleteControls);
		}

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">
		/// The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.
		/// </param>
		protected override void Render(HtmlTextWriter writer)
		{
			// Prints the grid
			writer.AddAttribute("id", ClientID);
			writer.AddAttribute("class", "dtg");

			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			ShowTableHeader.RenderControl(writer);
			ShowTableFooter.RenderControl(writer);
			NumberOfRows.RenderControl(writer);
			ContentSorting.RenderControl(writer);
			Grid.RenderControl(writer);
			Toolbar.RenderControl(writer);

			// Prints the insert, edit and delete controls);
			InsertControls.RenderControl(writer);
			EditControls.RenderControl(writer);
			DeleteControls.RenderControl(writer);

			writer.RenderEndTag();
		}
	}
}