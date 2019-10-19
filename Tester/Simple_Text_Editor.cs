using FarsiLibrary.Win;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Tester.Properties;

namespace Tester
{
	public class Simple_Text_Editor : Form
	{
		private enum ExplorerItemType
		{
			Class,
			Method,
			Property,
			Event
		}

		private class ExplorerItem
		{
			public ExplorerItemType type;

			public string title;

			public int position;
		}

		private class ExplorerItemComparer : IComparer<ExplorerItem>
		{
			public int Compare(ExplorerItem x, ExplorerItem y)
			{
				return x.title.CompareTo(y.title);
			}
		}

		private class DeclarationSnippet : SnippetAutocompleteItem
		{
			public DeclarationSnippet(string snippet)
				: base(snippet)
			{
			}

			public override CompareResult Compare(string fragmentText)
			{
				string str = Regex.Escape(fragmentText);
				if (Regex.IsMatch(Text, "\\b" + str, RegexOptions.IgnoreCase))
				{
					return CompareResult.Visible;
				}
				return CompareResult.Hidden;
			}
		}

		private class InsertSpaceSnippet : AutocompleteItem
		{
			private string pattern;

			public override string ToolTipTitle => Text;

			public InsertSpaceSnippet(string pattern)
				: base("")
			{
				this.pattern = pattern;
			}

			public InsertSpaceSnippet()
				: this("^(\\d+)([a-zA-Z_]+)(\\d*)$")
			{
			}

			public override CompareResult Compare(string fragmentText)
			{
				if (Regex.IsMatch(fragmentText, pattern))
				{
					Text = InsertSpaces(fragmentText);
					if (Text != fragmentText)
					{
						return CompareResult.Visible;
					}
				}
				return CompareResult.Hidden;
			}

			public string InsertSpaces(string fragment)
			{
				Match match = Regex.Match(fragment, pattern);
				if (match == null)
				{
					return fragment;
				}
				if (match.Groups[1].Value == "" && match.Groups[3].Value == "")
				{
					return fragment;
				}
				return (match.Groups[1].Value + " " + match.Groups[2].Value + " " + match.Groups[3].Value).Trim();
			}
		}

		private class InsertEnterSnippet : AutocompleteItem
		{
			private Place enterPlace = Place.Empty;

			public override string ToolTipTitle => "Insert line break after '}'";

			public InsertEnterSnippet()
				: base("[Line break]")
			{
			}

			public override CompareResult Compare(string fragmentText)
			{
				Range range = base.Parent.Fragment.Clone();
				while (range.Start.iChar > 0)
				{
					if (range.CharBeforeStart == '}')
					{
						enterPlace = range.Start;
						return CompareResult.Visible;
					}
					range.GoLeftThroughFolded();
				}
				return CompareResult.Hidden;
			}

			public override string GetTextForReplace()
			{
				Range fragment = base.Parent.Fragment;
				Place end = fragment.End;
				fragment.Start = enterPlace;
				fragment.End = fragment.End;
				return Environment.NewLine + fragment.Text;
			}

			public override void OnSelected(AutocompleteMenu popupMenu, SelectedEventArgs e)
			{
				base.OnSelected(popupMenu, e);
				if (base.Parent.Fragment.tb.AutoIndent)
				{
					base.Parent.Fragment.tb.DoAutoIndent();
				}
			}
		}

		private IContainer components = null;

		private MenuStrip msMain;

		private ToolStripMenuItem fileToolStripMenuItem;

		private ToolStripMenuItem openToolStripMenuItem;

		private ToolStripMenuItem saveToolStripMenuItem;

		private ToolStripMenuItem saveAsToolStripMenuItem;

		private ToolStripSeparator toolStripMenuItem1;

		private ToolStripMenuItem quitToolStripMenuItem;

		private StatusStrip ssMain;

		private ToolStrip tsMain;

		private FATabStrip tsFiles;

		private ToolStripMenuItem newToolStripMenuItem;

		private Splitter splitter1;

		private SaveFileDialog sfdMain;

		private OpenFileDialog ofdMain;

		private ContextMenuStrip cmMain;

		private ToolStripMenuItem cutToolStripMenuItem;

		private ToolStripMenuItem copyToolStripMenuItem;

		private ToolStripMenuItem pasteToolStripMenuItem;

		private ToolStripMenuItem selectAllToolStripMenuItem;

		private ToolStripSeparator toolStripMenuItem2;

		private ToolStripMenuItem undoToolStripMenuItem;

		private ToolStripMenuItem redoToolStripMenuItem;

		private System.Windows.Forms.Timer tmUpdateInterface;

		private ToolStripButton newToolStripButton;

		private ToolStripButton openToolStripButton;

		private ToolStripButton saveToolStripButton;

		private ToolStripButton printToolStripButton;

		private ToolStripSeparator toolStripSeparator;

		private ToolStripButton cutToolStripButton;

		private ToolStripButton copyToolStripButton;

		private ToolStripButton pasteToolStripButton;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripButton undoStripButton;

		private ToolStripButton redoStripButton;

		private ToolStripSeparator toolStripSeparator2;

		private ToolStripTextBox tbFind;

		private ToolStripLabel toolStripLabel1;

		private ToolStripSeparator toolStripMenuItem3;

		private ToolStripMenuItem findToolStripMenuItem;

		private ToolStripMenuItem replaceToolStripMenuItem;

		private DataGridView dgvObjectExplorer;

		private ToolStripButton backStripButton;

		private ToolStripButton forwardStripButton;

		private ToolStripSeparator toolStripSeparator3;

		private ToolStripSeparator toolStripSeparator4;

		private ToolStripSeparator toolStripSeparator5;

		private DataGridViewImageColumn clImage;

		private DataGridViewTextBoxColumn clName;

		private ToolStripStatusLabel lbWordUnderMouse;

		private ImageList ilAutocomplete;

		private ToolStripSeparator toolStripMenuItem4;

		private ToolStripMenuItem autoIndentSelectedTextToolStripMenuItem;

		private ToolStripButton btInvisibleChars;

		private ToolStripButton btHighlightCurrentLine;

		private ToolStripMenuItem commentSelectedToolStripMenuItem;

		private ToolStripMenuItem uncommentSelectedToolStripMenuItem;

		private ToolStripMenuItem cloneLinesToolStripMenuItem;

		private ToolStripMenuItem cloneLinesAndCommentToolStripMenuItem;

		private ToolStripSeparator toolStripSeparator6;

		private ToolStripButton bookmarkPlusButton;

		private ToolStripButton bookmarkMinusButton;

		private ToolStripDropDownButton gotoButton;

		private ToolStripButton btShowFoldingLines;

		private ToolStripSplitButton btZoom;

		private ToolStripMenuItem toolStripMenuItem11;

		private ToolStripMenuItem toolStripMenuItem10;

		private ToolStripMenuItem toolStripMenuItem9;

		private ToolStripMenuItem toolStripMenuItem8;

		private ToolStripMenuItem toolStripMenuItem7;

		private ToolStripMenuItem toolStripMenuItem6;

		private string[] keywords = new string[98]
		{
			"abstract",
			"as",
			"base",
			"bool",
			"break",
			"byte",
			"case",
			"catch",
			"char",
			"checked",
			"class",
			"const",
			"continue",
			"decimal",
			"default",
			"delegate",
			"do",
			"double",
			"else",
			"enum",
			"event",
			"explicit",
			"extern",
			"false",
			"finally",
			"fixed",
			"float",
			"for",
			"foreach",
			"goto",
			"if",
			"implicit",
			"in",
			"int",
			"interface",
			"internal",
			"is",
			"lock",
			"long",
			"namespace",
			"new",
			"null",
			"object",
			"operator",
			"out",
			"override",
			"params",
			"private",
			"protected",
			"public",
			"readonly",
			"ref",
			"return",
			"sbyte",
			"sealed",
			"short",
			"sizeof",
			"stackalloc",
			"static",
			"string",
			"struct",
			"switch",
			"this",
			"throw",
			"true",
			"try",
			"typeof",
			"uint",
			"ulong",
			"unchecked",
			"unsafe",
			"ushort",
			"using",
			"virtual",
			"void",
			"volatile",
			"while",
			"add",
			"alias",
			"ascending",
			"descending",
			"dynamic",
			"from",
			"get",
			"global",
			"group",
			"into",
			"join",
			"let",
			"orderby",
			"partial",
			"remove",
			"select",
			"set",
			"value",
			"var",
			"where",
			"yield"
		};

		private string[] methods = new string[4]
		{
			"Equals()",
			"GetHashCode()",
			"GetType()",
			"ToString()"
		};

		private string[] snippets = new string[6]
		{
			"if(^)\n{\n;\n}",
			"if(^)\n{\n;\n}\nelse\n{\n;\n}",
			"for(^;;)\n{\n;\n}",
			"while(^)\n{\n;\n}",
			"do\n{\n^;\n}while();",
			"switch(^)\n{\ncase : break;\n}"
		};

		private string[] declarationSnippets = new string[14]
		{
			"public class ^\n{\n}",
			"private class ^\n{\n}",
			"internal class ^\n{\n}",
			"public struct ^\n{\n;\n}",
			"private struct ^\n{\n;\n}",
			"internal struct ^\n{\n;\n}",
			"public void ^()\n{\n;\n}",
			"private void ^()\n{\n;\n}",
			"internal void ^()\n{\n;\n}",
			"protected void ^()\n{\n;\n}",
			"public ^{ get; set; }",
			"private ^{ get; set; }",
			"internal ^{ get; set; }",
			"protected ^{ get; set; }"
		};

		private Style invisibleCharsStyle = new InvisibleCharsRenderer(Pens.Gray);

		private Color currentLineColor = Color.FromArgb(100, 210, 210, 255);

		private Color changedLineColor = Color.FromArgb(255, 230, 230, 255);

		private Style sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(50, Color.Gray)));

		private List<ExplorerItem> explorerList = new List<ExplorerItem>();

		private bool tbFindChanged = false;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem printToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem1;
        private ToolStripMenuItem copyToolStripMenuItem1;
        private ToolStripMenuItem pasteToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem undoToolStripMenuItem1;
        private ToolStripMenuItem redoToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem findToolStripMenuItem1;
        private ToolStripMenuItem replaceToolStripMenuItem1;
        private ToolStripMenuItem selectAllToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripMenuItem nextDocumentToolStripMenuItem;
        private ToolStripMenuItem previousDocumentToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem fontToolStripMenuItem;
        private ToolStripMenuItem fullScreenToolStripMenuItem;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem restartToolStripMenuItem;
        private ToolStripMenuItem closeAllWindowsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripButton saveAllToolStripButton;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem View_DocumentSelector_MenuItem;
        private DateTime lastNavigatedDateTime = DateTime.Now;

		private FastColoredTextBox CurrentTB
		{
			get
			{
				if (tsFiles.SelectedItem == null)
				{
					return null;
				}
				return tsFiles.SelectedItem.Controls[0] as FastColoredTextBox;
			}
			set
			{
				tsFiles.SelectedItem = (value.Parent as FATabStripItem);
				value.Focus();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Simple_Text_Editor));
            this.msMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.lbWordUnderMouse = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tbFind = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsFiles = new FarsiLibrary.Win.FATabStrip();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.sfdMain = new System.Windows.Forms.SaveFileDialog();
            this.ofdMain = new System.Windows.Forms.OpenFileDialog();
            this.cmMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.autoIndentSelectedTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commentSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncommentSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneLinesAndCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tmUpdateInterface = new System.Windows.Forms.Timer(this.components);
            this.dgvObjectExplorer = new System.Windows.Forms.DataGridView();
            this.clImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.clName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ilAutocomplete = new System.Windows.Forms.ImageList(this.components);
            this.View_DocumentSelector_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveAllToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.printToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.cutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.btInvisibleChars = new System.Windows.Forms.ToolStripButton();
            this.btShowFoldingLines = new System.Windows.Forms.ToolStripButton();
            this.undoStripButton = new System.Windows.Forms.ToolStripButton();
            this.redoStripButton = new System.Windows.Forms.ToolStripButton();
            this.backStripButton = new System.Windows.Forms.ToolStripButton();
            this.forwardStripButton = new System.Windows.Forms.ToolStripButton();
            this.bookmarkPlusButton = new System.Windows.Forms.ToolStripButton();
            this.bookmarkMinusButton = new System.Windows.Forms.ToolStripButton();
            this.btHighlightCurrentLine = new System.Windows.Forms.ToolStripButton();
            this.gotoButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.nextDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllWindowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btZoom = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.msMain.SuspendLayout();
            this.ssMain.SuspendLayout();
            this.tsMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tsFiles)).BeginInit();
            this.cmMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvObjectExplorer)).BeginInit();
            this.SuspendLayout();
            // 
            // msMain
            // 
            this.msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.editToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.msMain.Location = new System.Drawing.Point(0, 0);
            this.msMain.Name = "msMain";
            this.msMain.Size = new System.Drawing.Size(769, 24);
            this.msMain.TabIndex = 0;
            this.msMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator7,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.printToolStripMenuItem,
            this.toolStripSeparator8,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(143, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 6);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(143, 6);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem1,
            this.copyToolStripMenuItem1,
            this.pasteToolStripMenuItem1,
            this.toolStripSeparator9,
            this.undoToolStripMenuItem1,
            this.redoToolStripMenuItem1,
            this.toolStripSeparator10,
            this.findToolStripMenuItem1,
            this.replaceToolStripMenuItem1,
            this.selectAllToolStripMenuItem1,
            this.toolStripSeparator11,
            this.nextDocumentToolStripMenuItem,
            this.previousDocumentToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(175, 6);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(175, 6);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(175, 6);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fontToolStripMenuItem,
            this.toolStripSeparator12,
            this.toolStripMenuItem5,
            this.View_DocumentSelector_MenuItem,
            this.toolStripSeparator13,
            this.fullScreenToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(172, 6);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(172, 6);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.restartToolStripMenuItem,
            this.toolStripSeparator14,
            this.closeAllWindowsToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(169, 6);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // ssMain
            // 
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbWordUnderMouse,
            this.btZoom});
            this.ssMain.Location = new System.Drawing.Point(0, 307);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(769, 22);
            this.ssMain.TabIndex = 2;
            this.ssMain.Text = "statusStrip1";
            // 
            // lbWordUnderMouse
            // 
            this.lbWordUnderMouse.AutoSize = false;
            this.lbWordUnderMouse.ForeColor = System.Drawing.Color.Gray;
            this.lbWordUnderMouse.Name = "lbWordUnderMouse";
            this.lbWordUnderMouse.Size = new System.Drawing.Size(699, 17);
            this.lbWordUnderMouse.Spring = true;
            this.lbWordUnderMouse.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripButton,
            this.openToolStripButton,
            this.saveToolStripButton,
            this.saveAllToolStripButton,
            this.printToolStripButton,
            this.toolStripSeparator3,
            this.cutToolStripButton,
            this.copyToolStripButton,
            this.pasteToolStripButton,
            this.btInvisibleChars,
            this.btShowFoldingLines,
            this.toolStripSeparator4,
            this.undoStripButton,
            this.redoStripButton,
            this.toolStripSeparator5,
            this.backStripButton,
            this.forwardStripButton,
            this.tbFind,
            this.toolStripLabel1,
            this.toolStripSeparator6,
            this.bookmarkPlusButton,
            this.bookmarkMinusButton,
            this.btHighlightCurrentLine,
            this.gotoButton});
            this.tsMain.Location = new System.Drawing.Point(0, 24);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(769, 25);
            this.tsMain.TabIndex = 3;
            this.tsMain.Text = "toolStrip1";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // tbFind
            // 
            this.tbFind.AcceptsReturn = true;
            this.tbFind.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tbFind.Name = "tbFind";
            this.tbFind.Size = new System.Drawing.Size(100, 25);
            this.tbFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbFind_KeyPress);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(36, 22);
            this.toolStripLabel1.Text = "Find: ";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsFiles
            // 
            this.tsFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tsFiles.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.tsFiles.Location = new System.Drawing.Point(175, 49);
            this.tsFiles.Name = "tsFiles";
            this.tsFiles.Size = new System.Drawing.Size(594, 258);
            this.tsFiles.TabIndex = 0;
            this.tsFiles.Text = "faTabStrip1";
            this.tsFiles.TabStripItemClosing += new FarsiLibrary.Win.TabStripItemClosingHandler(this.tsFiles_TabStripItemClosing);
            this.tsFiles.TabStripItemSelectionChanged += new FarsiLibrary.Win.TabStripItemChangedHandler(this.tsFiles_TabStripItemSelectionChanged);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(172, 49);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 258);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // sfdMain
            // 
            this.sfdMain.DefaultExt = "cs";
            this.sfdMain.Filter = "C# file(*.cs)|*.cs";
            // 
            // ofdMain
            // 
            this.ofdMain.DefaultExt = "cs";
            this.ofdMain.Filter = "C# file(*.cs)|*.cs";
            // 
            // cmMain
            // 
            this.cmMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.selectAllToolStripMenuItem,
            this.toolStripMenuItem2,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripMenuItem3,
            this.findToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.toolStripMenuItem4,
            this.autoIndentSelectedTextToolStripMenuItem,
            this.commentSelectedToolStripMenuItem,
            this.uncommentSelectedToolStripMenuItem,
            this.cloneLinesToolStripMenuItem,
            this.cloneLinesAndCommentToolStripMenuItem});
            this.cmMain.Name = "cmMain";
            this.cmMain.Size = new System.Drawing.Size(219, 308);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.selectAllToolStripMenuItem.Text = "Select all";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(215, 6);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(215, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.replaceToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(215, 6);
            // 
            // autoIndentSelectedTextToolStripMenuItem
            // 
            this.autoIndentSelectedTextToolStripMenuItem.Name = "autoIndentSelectedTextToolStripMenuItem";
            this.autoIndentSelectedTextToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.autoIndentSelectedTextToolStripMenuItem.Text = "AutoIndent selected text";
            this.autoIndentSelectedTextToolStripMenuItem.Click += new System.EventHandler(this.autoIndentSelectedTextToolStripMenuItem_Click);
            // 
            // commentSelectedToolStripMenuItem
            // 
            this.commentSelectedToolStripMenuItem.Name = "commentSelectedToolStripMenuItem";
            this.commentSelectedToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.commentSelectedToolStripMenuItem.Text = "Comment selected";
            this.commentSelectedToolStripMenuItem.Click += new System.EventHandler(this.commentSelectedToolStripMenuItem_Click);
            // 
            // uncommentSelectedToolStripMenuItem
            // 
            this.uncommentSelectedToolStripMenuItem.Name = "uncommentSelectedToolStripMenuItem";
            this.uncommentSelectedToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.uncommentSelectedToolStripMenuItem.Text = "Uncomment selected";
            this.uncommentSelectedToolStripMenuItem.Click += new System.EventHandler(this.uncommentSelectedToolStripMenuItem_Click);
            // 
            // cloneLinesToolStripMenuItem
            // 
            this.cloneLinesToolStripMenuItem.Name = "cloneLinesToolStripMenuItem";
            this.cloneLinesToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.cloneLinesToolStripMenuItem.Text = "Clone line(s)";
            this.cloneLinesToolStripMenuItem.Click += new System.EventHandler(this.cloneLinesToolStripMenuItem_Click);
            // 
            // cloneLinesAndCommentToolStripMenuItem
            // 
            this.cloneLinesAndCommentToolStripMenuItem.Name = "cloneLinesAndCommentToolStripMenuItem";
            this.cloneLinesAndCommentToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.cloneLinesAndCommentToolStripMenuItem.Text = "Clone line(s) and comment";
            this.cloneLinesAndCommentToolStripMenuItem.Click += new System.EventHandler(this.cloneLinesAndCommentToolStripMenuItem_Click);
            // 
            // tmUpdateInterface
            // 
            this.tmUpdateInterface.Enabled = true;
            this.tmUpdateInterface.Interval = 400;
            this.tmUpdateInterface.Tick += new System.EventHandler(this.tmUpdateInterface_Tick);
            // 
            // dgvObjectExplorer
            // 
            this.dgvObjectExplorer.AllowUserToAddRows = false;
            this.dgvObjectExplorer.AllowUserToDeleteRows = false;
            this.dgvObjectExplorer.AllowUserToResizeColumns = false;
            this.dgvObjectExplorer.AllowUserToResizeRows = false;
            this.dgvObjectExplorer.BackgroundColor = System.Drawing.Color.White;
            this.dgvObjectExplorer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvObjectExplorer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvObjectExplorer.ColumnHeadersVisible = false;
            this.dgvObjectExplorer.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clImage,
            this.clName});
            this.dgvObjectExplorer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dgvObjectExplorer.Dock = System.Windows.Forms.DockStyle.Left;
            this.dgvObjectExplorer.GridColor = System.Drawing.Color.White;
            this.dgvObjectExplorer.Location = new System.Drawing.Point(0, 49);
            this.dgvObjectExplorer.MultiSelect = false;
            this.dgvObjectExplorer.Name = "dgvObjectExplorer";
            this.dgvObjectExplorer.ReadOnly = true;
            this.dgvObjectExplorer.RowHeadersVisible = false;
            this.dgvObjectExplorer.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.dgvObjectExplorer.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgvObjectExplorer.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Green;
            this.dgvObjectExplorer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvObjectExplorer.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvObjectExplorer.Size = new System.Drawing.Size(172, 258);
            this.dgvObjectExplorer.TabIndex = 6;
            this.dgvObjectExplorer.VirtualMode = true;
            this.dgvObjectExplorer.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvObjectExplorer_CellMouseDoubleClick);
            this.dgvObjectExplorer.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.dgvObjectExplorer_CellValueNeeded);
            // 
            // clImage
            // 
            this.clImage.HeaderText = "Column2";
            this.clImage.MinimumWidth = 32;
            this.clImage.Name = "clImage";
            this.clImage.ReadOnly = true;
            this.clImage.Width = 32;
            // 
            // clName
            // 
            this.clName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.clName.HeaderText = "Column1";
            this.clName.Name = "clName";
            this.clName.ReadOnly = true;
            // 
            // ilAutocomplete
            // 
            this.ilAutocomplete.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilAutocomplete.ImageStream")));
            this.ilAutocomplete.TransparentColor = System.Drawing.Color.Transparent;
            this.ilAutocomplete.Images.SetKeyName(0, "script_16x16.png");
            this.ilAutocomplete.Images.SetKeyName(1, "app_16x16.png");
            this.ilAutocomplete.Images.SetKeyName(2, "1302166543_virtualbox.png");
            // 
            // View_DocumentSelector_MenuItem
            // 
            this.View_DocumentSelector_MenuItem.Checked = true;
            this.View_DocumentSelector_MenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.View_DocumentSelector_MenuItem.ForeColor = System.Drawing.Color.Black;
            this.View_DocumentSelector_MenuItem.Name = "View_DocumentSelector_MenuItem";
            this.View_DocumentSelector_MenuItem.Size = new System.Drawing.Size(175, 22);
            this.View_DocumentSelector_MenuItem.Text = "Tool Strip";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Checked = true;
            this.toolStripMenuItem5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem5.ForeColor = System.Drawing.Color.Black;
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(175, 22);
            this.toolStripMenuItem5.Text = "Document Selector";
            // 
            // newToolStripButton
            // 
            this.newToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newToolStripButton.Image = global::Properties.Resources.icons8_add_file_20__1_;
            this.newToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripButton.Name = "newToolStripButton";
            this.newToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.newToolStripButton.Text = "&New";
            this.newToolStripButton.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openToolStripButton.Image = global::Properties.Resources.icons8_opened_folder_20__1_;
            this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.openToolStripButton.Text = "&Open";
            this.openToolStripButton.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripButton.Image = global::Properties.Resources.icons8_save_20__1_;
            this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveToolStripButton.Text = "&Save";
            this.saveToolStripButton.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAllToolStripButton
            // 
            this.saveAllToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAllToolStripButton.Image = global::Properties.Resources.icons8_save_all_20__1_;
            this.saveAllToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAllToolStripButton.Name = "saveAllToolStripButton";
            this.saveAllToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.saveAllToolStripButton.Text = "toolStripButton1";
            // 
            // printToolStripButton
            // 
            this.printToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printToolStripButton.Image = global::Properties.Resources.icons8_print_20__2_;
            this.printToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printToolStripButton.Name = "printToolStripButton";
            this.printToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.printToolStripButton.Text = "&Print";
            this.printToolStripButton.Click += new System.EventHandler(this.printToolStripButton_Click);
            // 
            // cutToolStripButton
            // 
            this.cutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cutToolStripButton.Image = global::Properties.Resources.icons8_cut_20;
            this.cutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripButton.Name = "cutToolStripButton";
            this.cutToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.cutToolStripButton.Text = "C&ut";
            this.cutToolStripButton.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripButton
            // 
            this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.copyToolStripButton.Image = global::Properties.Resources.icons8_copy_20__1_;
            this.copyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripButton.Name = "copyToolStripButton";
            this.copyToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.copyToolStripButton.Text = "&Copy";
            this.copyToolStripButton.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripButton
            // 
            this.pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pasteToolStripButton.Image = global::Properties.Resources.icons8_paste_20__1_;
            this.pasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripButton.Name = "pasteToolStripButton";
            this.pasteToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.pasteToolStripButton.Text = "&Paste";
            this.pasteToolStripButton.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // btInvisibleChars
            // 
            this.btInvisibleChars.CheckOnClick = true;
            this.btInvisibleChars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btInvisibleChars.Image = global::Properties.Resources.icons8_paragraph_20;
            this.btInvisibleChars.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btInvisibleChars.Name = "btInvisibleChars";
            this.btInvisibleChars.Size = new System.Drawing.Size(23, 22);
            this.btInvisibleChars.ToolTipText = "Show invisible chars";
            this.btInvisibleChars.Click += new System.EventHandler(this.btInvisibleChars_Click);
            // 
            // btShowFoldingLines
            // 
            this.btShowFoldingLines.Checked = true;
            this.btShowFoldingLines.CheckOnClick = true;
            this.btShowFoldingLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btShowFoldingLines.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btShowFoldingLines.Image = ((System.Drawing.Image)(resources.GetObject("btShowFoldingLines.Image")));
            this.btShowFoldingLines.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btShowFoldingLines.Name = "btShowFoldingLines";
            this.btShowFoldingLines.Size = new System.Drawing.Size(23, 22);
            this.btShowFoldingLines.Text = "Show folding lines";
            this.btShowFoldingLines.Click += new System.EventHandler(this.btShowFoldingLines_Click);
            // 
            // undoStripButton
            // 
            this.undoStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.undoStripButton.Image = global::Properties.Resources.icons8_undo_20;
            this.undoStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.undoStripButton.Name = "undoStripButton";
            this.undoStripButton.Size = new System.Drawing.Size(23, 22);
            this.undoStripButton.Text = "Undo (Ctrl+Z)";
            this.undoStripButton.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoStripButton
            // 
            this.redoStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.redoStripButton.Image = global::Properties.Resources.icons8_redo_20;
            this.redoStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.redoStripButton.Name = "redoStripButton";
            this.redoStripButton.Size = new System.Drawing.Size(23, 22);
            this.redoStripButton.Text = "Redo (Ctrl+R)";
            this.redoStripButton.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // backStripButton
            // 
            this.backStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.backStripButton.Image = global::Properties.Resources.icons8_arrow_pointing_left_20;
            this.backStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.backStripButton.Name = "backStripButton";
            this.backStripButton.Size = new System.Drawing.Size(23, 22);
            this.backStripButton.Text = "Navigate Backward (Ctrl+ -)";
            this.backStripButton.Click += new System.EventHandler(this.backStripButton_Click);
            // 
            // forwardStripButton
            // 
            this.forwardStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.forwardStripButton.Image = global::Properties.Resources.icons8_arrow_20__1_;
            this.forwardStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.forwardStripButton.Name = "forwardStripButton";
            this.forwardStripButton.Size = new System.Drawing.Size(23, 22);
            this.forwardStripButton.Text = "Navigate Forward (Ctrl+Shift+ -)";
            this.forwardStripButton.Click += new System.EventHandler(this.forwardStripButton_Click);
            // 
            // bookmarkPlusButton
            // 
            this.bookmarkPlusButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bookmarkPlusButton.Image = global::Properties.Resources.icons8_bookmark_20;
            this.bookmarkPlusButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bookmarkPlusButton.Name = "bookmarkPlusButton";
            this.bookmarkPlusButton.Size = new System.Drawing.Size(23, 22);
            this.bookmarkPlusButton.Text = "Add bookmark (Ctrl-B)";
            this.bookmarkPlusButton.Click += new System.EventHandler(this.bookmarkPlusButton_Click);
            // 
            // bookmarkMinusButton
            // 
            this.bookmarkMinusButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bookmarkMinusButton.Image = global::Properties.Resources.icons8_minus_16;
            this.bookmarkMinusButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bookmarkMinusButton.Name = "bookmarkMinusButton";
            this.bookmarkMinusButton.Size = new System.Drawing.Size(23, 22);
            this.bookmarkMinusButton.Text = "Remove bookmark (Ctrl-Shift-B)";
            this.bookmarkMinusButton.Click += new System.EventHandler(this.bookmarkMinusButton_Click);
            // 
            // btHighlightCurrentLine
            // 
            this.btHighlightCurrentLine.CheckOnClick = true;
            this.btHighlightCurrentLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btHighlightCurrentLine.Image = global::Properties.Resources.icons8_marker_pen_20;
            this.btHighlightCurrentLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btHighlightCurrentLine.Name = "btHighlightCurrentLine";
            this.btHighlightCurrentLine.Size = new System.Drawing.Size(23, 22);
            this.btHighlightCurrentLine.Text = "Highlight current line";
            this.btHighlightCurrentLine.ToolTipText = "Highlight current line";
            this.btHighlightCurrentLine.Click += new System.EventHandler(this.btHighlightCurrentLine_Click);
            // 
            // gotoButton
            // 
            this.gotoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.gotoButton.Image = ((System.Drawing.Image)(resources.GetObject("gotoButton.Image")));
            this.gotoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.gotoButton.Name = "gotoButton";
            this.gotoButton.Size = new System.Drawing.Size(55, 22);
            this.gotoButton.Text = "Goto...";
            this.gotoButton.DropDownOpening += new System.EventHandler(this.gotoButton_DropDownOpening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Image = global::Properties.Resources.icons8_save_all_30;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.saveAsToolStripMenuItem.Text = "Save as ...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Image = global::Properties.Resources.icons8_print_20__2_;
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.printToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("quitToolStripMenuItem.Image")));
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.quitToolStripMenuItem.Text = "Exit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // fontToolStripMenuItem
            // 
            this.fontToolStripMenuItem.Image = global::Properties.Resources.icons8_choose_font_20;
            this.fontToolStripMenuItem.Name = "fontToolStripMenuItem";
            this.fontToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.fontToolStripMenuItem.Text = "Font";
            // 
            // fullScreenToolStripMenuItem
            // 
            this.fullScreenToolStripMenuItem.Image = global::Properties.Resources.icons8_fit_to_width_20;
            this.fullScreenToolStripMenuItem.Name = "fullScreenToolStripMenuItem";
            this.fullScreenToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.fullScreenToolStripMenuItem.Text = "Full Screen";
            // 
            // cutToolStripMenuItem1
            // 
            this.cutToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem1.Image")));
            this.cutToolStripMenuItem1.Name = "cutToolStripMenuItem1";
            this.cutToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.cutToolStripMenuItem1.Text = "Cut";
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem1.Image")));
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.copyToolStripMenuItem1.Text = "Copy";
            // 
            // pasteToolStripMenuItem1
            // 
            this.pasteToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem1.Image")));
            this.pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
            this.pasteToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.pasteToolStripMenuItem1.Text = "Paste";
            // 
            // undoToolStripMenuItem1
            // 
            this.undoToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("undoToolStripMenuItem1.Image")));
            this.undoToolStripMenuItem1.Name = "undoToolStripMenuItem1";
            this.undoToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.undoToolStripMenuItem1.Text = "Undo";
            // 
            // redoToolStripMenuItem1
            // 
            this.redoToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("redoToolStripMenuItem1.Image")));
            this.redoToolStripMenuItem1.Name = "redoToolStripMenuItem1";
            this.redoToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.redoToolStripMenuItem1.Text = "Redo";
            // 
            // findToolStripMenuItem1
            // 
            this.findToolStripMenuItem1.Image = global::Properties.Resources.icons8_search_20;
            this.findToolStripMenuItem1.Name = "findToolStripMenuItem1";
            this.findToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.findToolStripMenuItem1.Text = "Find";
            // 
            // replaceToolStripMenuItem1
            // 
            this.replaceToolStripMenuItem1.Image = global::Properties.Resources.icons8_find_and_replace_20__1_;
            this.replaceToolStripMenuItem1.Name = "replaceToolStripMenuItem1";
            this.replaceToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.replaceToolStripMenuItem1.Text = "Replace";
            // 
            // selectAllToolStripMenuItem1
            // 
            this.selectAllToolStripMenuItem1.Image = global::Properties.Resources.icons8_select_all_20;
            this.selectAllToolStripMenuItem1.Name = "selectAllToolStripMenuItem1";
            this.selectAllToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
            this.selectAllToolStripMenuItem1.Text = "Select All";
            // 
            // nextDocumentToolStripMenuItem
            // 
            this.nextDocumentToolStripMenuItem.Image = global::Properties.Resources.icons8_arrow_20__1_;
            this.nextDocumentToolStripMenuItem.Name = "nextDocumentToolStripMenuItem";
            this.nextDocumentToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.nextDocumentToolStripMenuItem.Text = "Next Document";
            // 
            // previousDocumentToolStripMenuItem
            // 
            this.previousDocumentToolStripMenuItem.Image = global::Properties.Resources.icons8_arrow_pointing_left_20;
            this.previousDocumentToolStripMenuItem.Name = "previousDocumentToolStripMenuItem";
            this.previousDocumentToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.previousDocumentToolStripMenuItem.Text = "Previous Document";
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Image = global::Properties.Resources.icons8_restart_20;
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.restartToolStripMenuItem.Text = "Restart";
            // 
            // closeAllWindowsToolStripMenuItem
            // 
            this.closeAllWindowsToolStripMenuItem.Image = global::Properties.Resources.icons8_close_window_20;
            this.closeAllWindowsToolStripMenuItem.Name = "closeAllWindowsToolStripMenuItem";
            this.closeAllWindowsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.closeAllWindowsToolStripMenuItem.Text = "Close All Windows";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::Properties.Resources.icons8_information_201;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.aboutToolStripMenuItem.Text = "About Simple Text Editor";
            // 
            // btZoom
            // 
            this.btZoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btZoom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem11,
            this.toolStripMenuItem10,
            this.toolStripMenuItem9,
            this.toolStripMenuItem8,
            this.toolStripMenuItem7,
            this.toolStripMenuItem6});
            this.btZoom.Image = ((System.Drawing.Image)(resources.GetObject("btZoom.Image")));
            this.btZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btZoom.Name = "btZoom";
            this.btZoom.Size = new System.Drawing.Size(55, 19);
            this.btZoom.Text = "Zoom";
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(102, 22);
            this.toolStripMenuItem11.Tag = "300";
            this.toolStripMenuItem11.Text = "300%";
            this.toolStripMenuItem11.Click += new System.EventHandler(this.Zoom_click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(102, 22);
            this.toolStripMenuItem10.Tag = "200";
            this.toolStripMenuItem10.Text = "200%";
            this.toolStripMenuItem10.Click += new System.EventHandler(this.Zoom_click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(102, 22);
            this.toolStripMenuItem9.Tag = "150";
            this.toolStripMenuItem9.Text = "150%";
            this.toolStripMenuItem9.Click += new System.EventHandler(this.Zoom_click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(102, 22);
            this.toolStripMenuItem8.Tag = "100";
            this.toolStripMenuItem8.Text = "100%";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.Zoom_click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(102, 22);
            this.toolStripMenuItem7.Tag = "50";
            this.toolStripMenuItem7.Text = "50%";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.Zoom_click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(102, 22);
            this.toolStripMenuItem6.Tag = "25";
            this.toolStripMenuItem6.Text = "25%";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.Zoom_click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Image = global::Tester.Properties.Resources.undo_16x16;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Image = global::Tester.Properties.Resources.redo_16x16;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // Simple_Text_Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(769, 329);
            this.Controls.Add(this.tsFiles);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.dgvObjectExplorer);
            this.Controls.Add(this.tsMain);
            this.Controls.Add(this.msMain);
            this.Controls.Add(this.ssMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.msMain;
            this.Name = "Simple_Text_Editor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple Text Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Simple_Text_Editor_FormClosing);
            this.msMain.ResumeLayout(false);
            this.msMain.PerformLayout();
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tsFiles)).EndInit();
            this.cmMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvObjectExplorer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		public Simple_Text_Editor()
		{
			InitializeComponent();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Simple_Text_Editor));
			copyToolStripMenuItem.Image = (Image)componentResourceManager.GetObject("copyToolStripButton.Image");
			cutToolStripMenuItem.Image = (Image)componentResourceManager.GetObject("cutToolStripButton.Image");
			pasteToolStripMenuItem.Image = (Image)componentResourceManager.GetObject("pasteToolStripButton.Image");
		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CreateTab(null);
		}

		private void CreateTab(string fileName)
		{
			try
			{
				FastColoredTextBox fastColoredTextBox = new FastColoredTextBox();
				fastColoredTextBox.Font = new Font("Consolas", 9.75f);
				fastColoredTextBox.ContextMenuStrip = cmMain;
				fastColoredTextBox.Dock = DockStyle.Fill;
				fastColoredTextBox.BorderStyle = BorderStyle.Fixed3D;
				fastColoredTextBox.LeftPadding = 17;
				fastColoredTextBox.Language = Language.CSharp;
				fastColoredTextBox.AddStyle(sameWordsStyle);
				FATabStripItem fATabStripItem = new FATabStripItem((fileName != null) ? Path.GetFileName(fileName) : "[new]", fastColoredTextBox);
				fATabStripItem.Tag = fileName;
				if (fileName != null)
				{
					fastColoredTextBox.OpenFile(fileName);
				}
				fastColoredTextBox.Tag = new TbInfo();
				tsFiles.AddTab(fATabStripItem);
				tsFiles.SelectedItem = fATabStripItem;
				fastColoredTextBox.Focus();
				fastColoredTextBox.DelayedTextChangedInterval = 1000;
				fastColoredTextBox.DelayedEventsInterval = 500;
				fastColoredTextBox.TextChangedDelayed += tb_TextChangedDelayed;
				fastColoredTextBox.SelectionChangedDelayed += tb_SelectionChangedDelayed;
				fastColoredTextBox.KeyDown += tb_KeyDown;
				fastColoredTextBox.MouseMove += tb_MouseMove;
				fastColoredTextBox.ChangedLineColor = changedLineColor;
				if (btHighlightCurrentLine.Checked)
				{
					fastColoredTextBox.CurrentLineColor = currentLineColor;
				}
				fastColoredTextBox.ShowFoldingLines = btShowFoldingLines.Checked;
				fastColoredTextBox.HighlightingRangeType = HighlightingRangeType.VisibleRange;
				AutocompleteMenu autocompleteMenu = new AutocompleteMenu(fastColoredTextBox);
				autocompleteMenu.Items.ImageList = ilAutocomplete;
				autocompleteMenu.Opening += popupMenu_Opening;
				BuildAutocompleteMenu(autocompleteMenu);
				(fastColoredTextBox.Tag as TbInfo).popupMenu = autocompleteMenu;
			}
			catch (Exception ex)
			{
				if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand) == DialogResult.Retry)
				{
					CreateTab(fileName);
				}
			}
		}

		private void popupMenu_Opening(object sender, CancelEventArgs e)
		{
			int styleIndex = CurrentTB.GetStyleIndex(CurrentTB.SyntaxHighlighter.GreenStyle);
			if (styleIndex >= 0 && CurrentTB.Selection.Start.iChar > 0)
			{
				FastColoredTextBoxNS.Char @char = CurrentTB[CurrentTB.Selection.Start.iLine][CurrentTB.Selection.Start.iChar - 1];
				StyleIndex styleIndex2 = Range.ToStyleIndex(styleIndex);
				if ((@char.style & styleIndex2) != 0)
				{
					e.Cancel = true;
				}
			}
		}

		private void BuildAutocompleteMenu(AutocompleteMenu popupMenu)
		{
			List<AutocompleteItem> list = new List<AutocompleteItem>();
			string[] array = snippets;
			foreach (string snippet in array)
			{
				list.Add(new SnippetAutocompleteItem(snippet)
				{
					ImageIndex = 1
				});
			}
			array = declarationSnippets;
			foreach (string snippet in array)
			{
				list.Add(new DeclarationSnippet(snippet)
				{
					ImageIndex = 0
				});
			}
			array = methods;
			foreach (string snippet in array)
			{
				list.Add(new MethodAutocompleteItem(snippet)
				{
					ImageIndex = 2
				});
			}
			array = keywords;
			foreach (string snippet in array)
			{
				list.Add(new AutocompleteItem(snippet));
			}
			list.Add(new InsertSpaceSnippet());
			list.Add(new InsertSpaceSnippet("^(\\w+)([=<>!:]+)(\\w+)$"));
			list.Add(new InsertEnterSnippet());
			popupMenu.Items.SetAutocompleteItems(list);
			popupMenu.SearchPattern = "[\\w\\.:=!<>]";
		}

		private void tb_MouseMove(object sender, MouseEventArgs e)
		{
			FastColoredTextBox fastColoredTextBox = sender as FastColoredTextBox;
			Place place = fastColoredTextBox.PointToPlace(e.Location);
			Range range = new Range(fastColoredTextBox, place, place);
			string text = range.GetFragment("[a-zA-Z]").Text;
			lbWordUnderMouse.Text = text;
		}

		private void tb_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control && e.KeyCode == Keys.OemMinus)
			{
				NavigateBackward();
				e.Handled = true;
			}
			if (e.Modifiers == (Keys.Shift | Keys.Control) && e.KeyCode == Keys.OemMinus)
			{
				NavigateForward();
				e.Handled = true;
			}
			if (e.KeyData == (Keys)131147)
			{
				(CurrentTB.Tag as TbInfo).popupMenu.Show(true);
				e.Handled = true;
			}
		}

		private void tb_SelectionChangedDelayed(object sender, EventArgs e)
		{
			FastColoredTextBox fastColoredTextBox = sender as FastColoredTextBox;
			if (fastColoredTextBox.Selection.IsEmpty && fastColoredTextBox.Selection.Start.iLine < fastColoredTextBox.LinesCount && lastNavigatedDateTime != fastColoredTextBox[fastColoredTextBox.Selection.Start.iLine].LastVisit)
			{
				fastColoredTextBox[fastColoredTextBox.Selection.Start.iLine].LastVisit = DateTime.Now;
				lastNavigatedDateTime = fastColoredTextBox[fastColoredTextBox.Selection.Start.iLine].LastVisit;
			}
			fastColoredTextBox.VisibleRange.ClearStyle(sameWordsStyle);
			if (!fastColoredTextBox.Selection.IsEmpty)
			{
				return;
			}
			Range fragment = fastColoredTextBox.Selection.GetFragment("\\w");
			string text = fragment.Text;
			if (text.Length == 0)
			{
				return;
			}
			Range[] array = fastColoredTextBox.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
			if (array.Length > 1)
			{
				Range[] array2 = array;
				foreach (Range range in array2)
				{
					range.SetStyle(sameWordsStyle);
				}
			}
		}

		private void tb_TextChangedDelayed(object sender, TextChangedEventArgs e)
		{
			FastColoredTextBox fastColoredTextBox = sender as FastColoredTextBox;
			string text = (sender as FastColoredTextBox).Text;
			ThreadPool.QueueUserWorkItem(delegate
			{
				ReBuildObjectExplorer(text);
			});
			HighlightInvisibleChars(e.ChangedRange);
		}

		private void HighlightInvisibleChars(Range range)
		{
			range.ClearStyle(invisibleCharsStyle);
			if (btInvisibleChars.Checked)
			{
				range.SetStyle(invisibleCharsStyle, ".$|.\\r\\n|\\s");
			}
		}

		private void ReBuildObjectExplorer(string text)
		{
			try
			{
				List<ExplorerItem> list = new List<ExplorerItem>();
				int num = -1;
				Regex regex = new Regex("^(?<range>[\\w\\s]+\\b(class|struct|enum|interface)\\s+[\\w<>,\\s]+)|^\\s*(public|private|internal|protected)[^\\n]+(\\n?\\s*{|;)?", RegexOptions.Multiline);
				foreach (Match item in regex.Matches(text))
				{
					try
					{
						string text2 = item.Value;
						int num2 = text2.IndexOfAny(new char[3]
						{
							'=',
							'{',
							';'
						});
						if (num2 >= 0)
						{
							text2 = text2.Substring(0, num2);
						}
						text2 = text2.Trim();
						ExplorerItem explorerItem = new ExplorerItem();
						explorerItem.title = text2;
						explorerItem.position = item.Index;
						ExplorerItem explorerItem2 = explorerItem;
						if (Regex.IsMatch(explorerItem2.title, "\\b(class|struct|enum|interface)\\b"))
						{
							explorerItem2.title = explorerItem2.title.Substring(explorerItem2.title.LastIndexOf(' ')).Trim();
							explorerItem2.type = ExplorerItemType.Class;
							list.Sort(num + 1, list.Count - (num + 1), new ExplorerItemComparer());
							num = list.Count;
							goto IL_02ad;
						}
						if (explorerItem2.title.Contains(" event "))
						{
							int startIndex = explorerItem2.title.LastIndexOf(' ');
							explorerItem2.title = explorerItem2.title.Substring(startIndex).Trim();
							explorerItem2.type = ExplorerItemType.Event;
							goto IL_02ad;
						}
						string[] array;
						if (explorerItem2.title.Contains("("))
						{
							array = explorerItem2.title.Split('(');
							explorerItem2.title = array[0].Substring(array[0].LastIndexOf(' ')).Trim() + "(" + array[1];
							explorerItem2.type = ExplorerItemType.Method;
							goto IL_02ad;
						}
						if (!explorerItem2.title.EndsWith("]"))
						{
							int startIndex = explorerItem2.title.LastIndexOf(' ');
							explorerItem2.title = explorerItem2.title.Substring(startIndex).Trim();
							explorerItem2.type = ExplorerItemType.Property;
							goto IL_02ad;
						}
						array = explorerItem2.title.Split('[');
						if (array.Length >= 2)
						{
							explorerItem2.title = array[0].Substring(array[0].LastIndexOf(' ')).Trim() + "[" + array[1];
							explorerItem2.type = ExplorerItemType.Method;
							goto IL_02ad;
						}
						goto end_IL_004c;
						IL_02ad:
						list.Add(explorerItem2);
						end_IL_004c:;
					}
					catch
					{
					}
				}
				list.Sort(num + 1, list.Count - (num + 1), new ExplorerItemComparer());
				BeginInvoke((Action)delegate
				{
					explorerList = list;
					dgvObjectExplorer.RowCount = explorerList.Count;
					dgvObjectExplorer.Invalidate();
				});
			}
			catch
			{
			}
		}

		private void tsFiles_TabStripItemClosing(TabStripItemClosingEventArgs e)
		{
			if (!(e.Item.Controls[0] as FastColoredTextBox).IsChanged)
			{
				return;
			}
			switch (MessageBox.Show("Do you want save " + e.Item.Title + " ?", "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk))
			{
			case DialogResult.Yes:
				if (!Save(e.Item))
				{
					e.Cancel = true;
				}
				break;
			case DialogResult.Cancel:
				e.Cancel = true;
				break;
			}
		}

		private bool Save(FATabStripItem tab)
		{
			FastColoredTextBox fastColoredTextBox = tab.Controls[0] as FastColoredTextBox;
			if (tab.Tag == null)
			{
				if (sfdMain.ShowDialog() != DialogResult.OK)
				{
					return false;
				}
				tab.Title = Path.GetFileName(sfdMain.FileName);
				tab.Tag = sfdMain.FileName;
			}
			try
			{
				File.WriteAllText(tab.Tag as string, fastColoredTextBox.Text);
				fastColoredTextBox.IsChanged = false;
			}
			catch (Exception ex)
			{
				if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand) == DialogResult.Retry)
				{
					return Save(tab);
				}
				return false;
			}
			fastColoredTextBox.Invalidate();
			return true;
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tsFiles.SelectedItem != null)
			{
				Save(tsFiles.SelectedItem);
			}
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tsFiles.SelectedItem != null)
			{
				string text = tsFiles.SelectedItem.Tag as string;
				tsFiles.SelectedItem.Tag = null;
				if (!Save(tsFiles.SelectedItem) && text != null)
				{
					tsFiles.SelectedItem.Tag = text;
					tsFiles.SelectedItem.Title = Path.GetFileName(text);
				}
			}
		}

		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (ofdMain.ShowDialog() == DialogResult.OK)
			{
				CreateTab(ofdMain.FileName);
			}
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.Cut();
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.Copy();
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.Paste();
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.Selection.SelectAll();
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentTB.UndoEnabled)
			{
				CurrentTB.Undo();
			}
		}

		private void redoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentTB.RedoEnabled)
			{
				CurrentTB.Redo();
			}
		}

		private void tmUpdateInterface_Tick(object sender, EventArgs e)
		{
			try
			{
				if (CurrentTB != null && tsFiles.Items.Count > 0)
				{
					FastColoredTextBox currentTB = CurrentTB;
					ToolStripButton toolStripButton = undoStripButton;
					bool enabled = undoToolStripMenuItem.Enabled = currentTB.UndoEnabled;
					toolStripButton.Enabled = enabled;
					ToolStripButton toolStripButton2 = redoStripButton;
					enabled = (redoToolStripMenuItem.Enabled = currentTB.RedoEnabled);
					toolStripButton2.Enabled = enabled;
					ToolStripButton toolStripButton3 = saveToolStripButton;
					enabled = (saveToolStripMenuItem.Enabled = currentTB.IsChanged);
					toolStripButton3.Enabled = enabled;
					saveAsToolStripMenuItem.Enabled = true;
					ToolStripButton toolStripButton4 = pasteToolStripButton;
					enabled = (pasteToolStripMenuItem.Enabled = true);
					toolStripButton4.Enabled = enabled;
					ToolStripButton toolStripButton5 = cutToolStripButton;
					ToolStripMenuItem toolStripMenuItem = cutToolStripMenuItem;
					ToolStripButton toolStripButton6 = copyToolStripButton;
					enabled = (copyToolStripMenuItem.Enabled = !currentTB.Selection.IsEmpty);
					enabled = (toolStripButton6.Enabled = enabled);
					enabled = (toolStripMenuItem.Enabled = enabled);
					toolStripButton5.Enabled = enabled;
					printToolStripButton.Enabled = true;
				}
				else
				{
					ToolStripButton toolStripButton7 = saveToolStripButton;
					bool enabled = saveToolStripMenuItem.Enabled = false;
					toolStripButton7.Enabled = enabled;
					saveAsToolStripMenuItem.Enabled = false;
					ToolStripButton toolStripButton8 = cutToolStripButton;
					ToolStripMenuItem toolStripMenuItem2 = cutToolStripMenuItem;
					ToolStripButton toolStripButton9 = copyToolStripButton;
					enabled = (copyToolStripMenuItem.Enabled = false);
					enabled = (toolStripButton9.Enabled = enabled);
					enabled = (toolStripMenuItem2.Enabled = enabled);
					toolStripButton8.Enabled = enabled;
					ToolStripButton toolStripButton10 = pasteToolStripButton;
					enabled = (pasteToolStripMenuItem.Enabled = false);
					toolStripButton10.Enabled = enabled;
					printToolStripButton.Enabled = false;
					ToolStripButton toolStripButton11 = undoStripButton;
					enabled = (undoToolStripMenuItem.Enabled = false);
					toolStripButton11.Enabled = enabled;
					ToolStripButton toolStripButton12 = redoStripButton;
					enabled = (redoToolStripMenuItem.Enabled = false);
					toolStripButton12.Enabled = enabled;
					dgvObjectExplorer.RowCount = 0;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private void printToolStripButton_Click(object sender, EventArgs e)
		{
			if (CurrentTB != null)
			{
				PrintDialogSettings printDialogSettings = new PrintDialogSettings();
				printDialogSettings.Title = tsFiles.SelectedItem.Title;
				printDialogSettings.Header = "&b&w&b";
				printDialogSettings.Footer = "&b&p";
				CurrentTB.Print(printDialogSettings);
			}
		}

		private void tbFind_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r' && CurrentTB != null)
			{
				Range range = tbFindChanged ? CurrentTB.Range.Clone() : CurrentTB.Selection.Clone();
				tbFindChanged = false;
				range.End = new Place(CurrentTB[CurrentTB.LinesCount - 1].Count, CurrentTB.LinesCount - 1);
				string regexPattern = Regex.Escape(tbFind.Text);
				using (IEnumerator<Range> enumerator = range.GetRanges(regexPattern).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Range current = enumerator.Current;
						current.Inverse();
						CurrentTB.Selection = current;
						CurrentTB.DoSelectionVisible();
						return;
					}
				}
				MessageBox.Show("Not found.");
			}
			else
			{
				tbFindChanged = true;
			}
		}

		private void findToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.ShowFindDialog();
		}

		private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.ShowReplaceDialog();
		}

		private void Simple_Text_Editor_FormClosing(object sender, FormClosingEventArgs e)
		{
			List<FATabStripItem> list = new List<FATabStripItem>();
			foreach (FATabStripItem item2 in tsFiles.Items)
			{
				list.Add(item2);
			}
			foreach (FATabStripItem item3 in list)
			{
				TabStripItemClosingEventArgs tabStripItemClosingEventArgs = new TabStripItemClosingEventArgs(item3);
				tsFiles_TabStripItemClosing(tabStripItemClosingEventArgs);
				if (tabStripItemClosingEventArgs.Cancel)
				{
					e.Cancel = true;
					break;
				}
				tsFiles.RemoveTab(item3);
			}
		}

		private void dgvObjectExplorer_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (CurrentTB != null)
			{
				ExplorerItem explorerItem = explorerList[e.RowIndex];
				CurrentTB.GoEnd();
				CurrentTB.SelectionStart = explorerItem.position;
				CurrentTB.DoSelectionVisible();
				CurrentTB.Focus();
			}
		}

		private void dgvObjectExplorer_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				ExplorerItem explorerItem = explorerList[e.RowIndex];
				if (e.ColumnIndex == 1)
				{
					e.Value = explorerItem.title;
				}
				else
				{
					switch (explorerItem.type)
					{
					case ExplorerItemType.Class:
						e.Value = Resources.class_libraries;
						break;
					case ExplorerItemType.Method:
						e.Value = Resources.box;
						break;
					case ExplorerItemType.Event:
						e.Value = Resources.lightning;
						break;
					case ExplorerItemType.Property:
						e.Value = Resources.property;
						break;
					}
				}
			}
			catch
			{
			}
		}

		private void tsFiles_TabStripItemSelectionChanged(TabStripItemChangedEventArgs e)
		{
			if (CurrentTB != null)
			{
				CurrentTB.Focus();
				string text = CurrentTB.Text;
				ThreadPool.QueueUserWorkItem(delegate
				{
					ReBuildObjectExplorer(text);
				});
			}
		}

		private void backStripButton_Click(object sender, EventArgs e)
		{
			NavigateBackward();
		}

		private void forwardStripButton_Click(object sender, EventArgs e)
		{
			NavigateForward();
		}

		private bool NavigateBackward()
		{
			DateTime t = default(DateTime);
			int num = -1;
			FastColoredTextBox fastColoredTextBox = null;
			for (int i = 0; i < tsFiles.Items.Count; i++)
			{
				FastColoredTextBox fastColoredTextBox2 = tsFiles.Items[i].Controls[0] as FastColoredTextBox;
				for (int j = 0; j < fastColoredTextBox2.LinesCount; j++)
				{
					if (fastColoredTextBox2[j].LastVisit < lastNavigatedDateTime && fastColoredTextBox2[j].LastVisit > t)
					{
						t = fastColoredTextBox2[j].LastVisit;
						num = j;
						fastColoredTextBox = fastColoredTextBox2;
					}
				}
			}
			if (num >= 0)
			{
				tsFiles.SelectedItem = (fastColoredTextBox.Parent as FATabStripItem);
				fastColoredTextBox.Navigate(num);
				lastNavigatedDateTime = fastColoredTextBox[num].LastVisit;
				Console.WriteLine("Backward: " + lastNavigatedDateTime);
				fastColoredTextBox.Focus();
				fastColoredTextBox.Invalidate();
				return true;
			}
			return false;
		}

		private bool NavigateForward()
		{
			DateTime t = DateTime.Now;
			int num = -1;
			FastColoredTextBox fastColoredTextBox = null;
			for (int i = 0; i < tsFiles.Items.Count; i++)
			{
				FastColoredTextBox fastColoredTextBox2 = tsFiles.Items[i].Controls[0] as FastColoredTextBox;
				for (int j = 0; j < fastColoredTextBox2.LinesCount; j++)
				{
					if (fastColoredTextBox2[j].LastVisit > lastNavigatedDateTime && fastColoredTextBox2[j].LastVisit < t)
					{
						t = fastColoredTextBox2[j].LastVisit;
						num = j;
						fastColoredTextBox = fastColoredTextBox2;
					}
				}
			}
			if (num >= 0)
			{
				tsFiles.SelectedItem = (fastColoredTextBox.Parent as FATabStripItem);
				fastColoredTextBox.Navigate(num);
				lastNavigatedDateTime = fastColoredTextBox[num].LastVisit;
				Console.WriteLine("Forward: " + lastNavigatedDateTime);
				fastColoredTextBox.Focus();
				fastColoredTextBox.Invalidate();
				return true;
			}
			return false;
		}

		private void autoIndentSelectedTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.DoAutoIndent();
		}

		private void btInvisibleChars_Click(object sender, EventArgs e)
		{
			foreach (FATabStripItem item in tsFiles.Items)
			{
				HighlightInvisibleChars((item.Controls[0] as FastColoredTextBox).Range);
			}
			if (CurrentTB != null)
			{
				CurrentTB.Invalidate();
			}
		}

		private void btHighlightCurrentLine_Click(object sender, EventArgs e)
		{
			foreach (FATabStripItem item in tsFiles.Items)
			{
				if (btHighlightCurrentLine.Checked)
				{
					(item.Controls[0] as FastColoredTextBox).CurrentLineColor = currentLineColor;
				}
				else
				{
					(item.Controls[0] as FastColoredTextBox).CurrentLineColor = Color.Transparent;
				}
			}
			if (CurrentTB != null)
			{
				CurrentTB.Invalidate();
			}
		}

		private void commentSelectedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.InsertLinePrefix("//");
		}

		private void uncommentSelectedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.RemoveLinePrefix("//");
		}

		private void cloneLinesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.Selection.Expand();
			string text = Environment.NewLine + CurrentTB.Selection.Text;
			CurrentTB.Selection.Start = CurrentTB.Selection.End;
			CurrentTB.InsertText(text);
		}

		private void cloneLinesAndCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrentTB.BeginAutoUndo();
			CurrentTB.Selection.Expand();
			string text = Environment.NewLine + CurrentTB.Selection.Text;
			CurrentTB.InsertLinePrefix("//");
			CurrentTB.Selection.Start = CurrentTB.Selection.End;
			CurrentTB.InsertText(text);
			CurrentTB.EndAutoUndo();
		}

		private void bookmarkPlusButton_Click(object sender, EventArgs e)
		{
			if (CurrentTB != null)
			{
				CurrentTB.BookmarkLine(CurrentTB.Selection.Start.iLine);
			}
		}

		private void bookmarkMinusButton_Click(object sender, EventArgs e)
		{
			if (CurrentTB != null)
			{
				CurrentTB.UnbookmarkLine(CurrentTB.Selection.Start.iLine);
			}
		}

		private void gotoButton_DropDownOpening(object sender, EventArgs e)
		{
			gotoButton.DropDownItems.Clear();
			foreach (Control item in tsFiles.Items)
			{
				FastColoredTextBox fastColoredTextBox = item.Controls[0] as FastColoredTextBox;
				foreach (Bookmark bookmark2 in fastColoredTextBox.Bookmarks)
				{
					ToolStripItem toolStripItem = gotoButton.DropDownItems.Add(bookmark2.Name + " [" + Path.GetFileNameWithoutExtension(item.Tag as string) + "]");
					toolStripItem.Tag = bookmark2;
					toolStripItem.Click += delegate(object o, EventArgs a)
					{
						Bookmark bookmark = (Bookmark)(o as ToolStripItem).Tag;
						try
						{
							CurrentTB = bookmark.TB;
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message);
							return;
						}
						bookmark.DoVisible();
					};
				}
			}
		}

		private void btShowFoldingLines_Click(object sender, EventArgs e)
		{
			foreach (FATabStripItem item in tsFiles.Items)
			{
				(item.Controls[0] as FastColoredTextBox).ShowFoldingLines = btShowFoldingLines.Checked;
			}
			if (CurrentTB != null)
			{
				CurrentTB.Invalidate();
			}
		}

		private void Zoom_click(object sender, EventArgs e)
		{
			if (CurrentTB != null)
			{
				CurrentTB.Zoom = int.Parse((sender as ToolStripItem).Tag.ToString());
			}
		}

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentTB != null)
            {
                PrintDialogSettings printDialogSettings = new PrintDialogSettings();
                printDialogSettings.Title = tsFiles.SelectedItem.Title;
                printDialogSettings.Header = "&b&w&b";
                printDialogSettings.Footer = "&b&p";
                CurrentTB.Print(printDialogSettings);
            }
        }
    }
}
