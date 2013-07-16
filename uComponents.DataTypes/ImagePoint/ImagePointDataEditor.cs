﻿using System;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using uComponents.Core;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using Umbraco.Web;
using umbraco.editorControls;
using System.Web.UI.HtmlControls;
using DefaultData = umbraco.cms.businesslogic.datatype.DefaultData;

[assembly: WebResource("uComponents.DataTypes.ImagePoint.ImagePoint.js", Constants.MediaTypeNames.Application.JavaScript)]
namespace uComponents.DataTypes.ImagePoint
{
    /// <summary>
    /// Image Point Data Type
    /// </summary>
    public class ImagePointDataEditor : CompositeControl, IDataEditor
    {
        /// <summary>
        /// Field for the data.
        /// </summary>
        private IData data;

        /// <summary>
        /// Field for the options.
        /// </summary>
        private ImagePointOptions options;

        /// <summary>
        /// Wrapping div
        /// </summary>
        private HtmlGenericControl div = new HtmlGenericControl("div");

        /// <summary>
        /// X coordinate
        /// </summary>
        private TextBox xTextBox = new TextBox();

        /// <summary>
        /// Y coordinate
        /// </summary>
        private TextBox yTextBox = new TextBox();

        /// <summary>
        /// Image tag used to define the x, y area
        /// </summary>
        private Image image = new Image();

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePointDataEditor"/> class. 
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="options">The options.</param>
        internal ImagePointDataEditor(IData data, ImagePointOptions options)
        {
            this.data = data;
            this.options = options;
        }

        /// <summary>
        /// Gets a value indicating whether [treat as rich text editor].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool TreatAsRichTextEditor
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show label].
        /// </summary>
        /// <value><c>true</c> if [show label]; otherwise, <c>false</c>.</value>
        public virtual bool ShowLabel
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the editor.
        /// </summary>
        /// <value>The editor.</value>
        public Control Editor
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the id of the current (content, media or member) on which this is a property
        /// </summary>
        private int CurrentContentId
        {
            get
            {
                return ((DefaultData)this.data).NodeId;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            /*
             *  <div>
             *      <input type="text" />
             *      <input type="text" />
             *      <img src="" (width="") (height="") />
             *  </div>
             * 
             */

            this.xTextBox.ID = "xTextBox";
            this.xTextBox.Width = 30;
            this.xTextBox.MaxLength = 4;                        

            this.yTextBox.ID = "yTextBox";
            this.yTextBox.Width = 30;
            this.yTextBox.MaxLength = 4;

            if (!string.IsNullOrWhiteSpace(this.options.ImagePropertyAlias))
            {
                try
                {
                    string imageUrl = null;

                    // looking for the specified property
                    switch (uQuery.GetUmbracoObjectType(this.CurrentContentId))
                    {
                        case uQuery.UmbracoObjectType.Document:
                            imageUrl = uQuery.GetDocument(this.CurrentContentId)
                                                .GetAncestorOrSelfDocuments()
                                                .First(x => x.HasProperty(this.options.ImagePropertyAlias))
                                                .GetProperty<string>(this.options.ImagePropertyAlias);
                            break;

                        case uQuery.UmbracoObjectType.Media:
                            imageUrl = uQuery.GetMedia(this.CurrentContentId)
                                                .GetAncestorOrSelfMedia()
                                                .First(x => x.HasProperty(this.options.ImagePropertyAlias))
                                                .GetProperty<string>(this.options.ImagePropertyAlias);
                            break;

                        case uQuery.UmbracoObjectType.Member:
                            imageUrl = uQuery.GetMember(this.CurrentContentId).GetProperty<string>(this.options.ImagePropertyAlias);

                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        this.image.ImageUrl = imageUrl;
                    }
                }
                catch
                {
                    // node, media or member with specified property couldn't be found

                    // TODO: if debug mode on, then thow exception, else be silent
                }
            }
           
            if (this.options.Width > 0)
            {
                this.image.Width = this.options.Width;
            }

            if (this.options.Height > 0)
            {
                this.image.Height = this.options.Height;
            }

            this.div.Controls.Add(new Literal() { Text = "X " });
            this.div.Controls.Add(this.xTextBox);
            this.div.Controls.Add(new Literal() { Text = " Y " });
            this.div.Controls.Add(this.yTextBox);
            this.div.Controls.Add(new HtmlGenericControl("br"));
            this.div.Controls.Add(this.image);

            this.Controls.Add(this.div);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.EnsureChildControls();

            if (!this.Page.IsPostBack && this.data.Value != null)
            {
                // set the x and y textboxes
                string[] coordinates = this.data.Value.ToString().Split(',');
                if (coordinates.Length == 2)
                {
                    this.xTextBox.Text = coordinates[0];
                    this.yTextBox.Text = coordinates[1];
                }
            }

            this.RegisterEmbeddedClientResource("uComponents.DataTypes.ImagePoint.ImagePoint.js", ClientDependencyType.Javascript);

            string startupScript = @"
                <script language='javascript' type='text/javascript'>
                    $(document).ready(function () {
                        ImagePoint.init(jQuery('div#" + this.div.ClientID + @"'));
                    });
                </script>";

            ScriptManager.RegisterStartupScript(this, typeof(ImagePointDataEditor), this.ClientID + "_init", startupScript, false);
        }

        /// <summary>
        /// Called by Umbraco when saving the node
        /// </summary>
        public void Save()
        {
            this.data.Value = this.xTextBox.Text + "," + this.yTextBox.Text;
        }
    }
}
