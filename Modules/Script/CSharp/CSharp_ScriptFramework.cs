﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 10.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace VixenModules.Script.CSharp
{
    using Vixen.Sys;
    using Vixen.Module.Effect;
    using System.Linq;
    using System.Collections.Generic;
    using System;
    
    
    #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class CSharp_ScriptFramework : CSharp_ScriptFrameworkBase
    {
        public virtual string TransformText()
        {
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write(@"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Dynamic;
using Vixen.Sys;
using Vixen.Module.SequenceType;
using Vixen.Module.Effect;
using Vixen.Module.Script;
using Vixen.Commands;

namespace ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Namespace));
            
            #line default
            #line hidden
            this.Write(" {\n\tpublic partial class ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ClassName));
            
            #line default
            #line hidden
            this.Write(" : UserScriptHost {\n\t\tprivate UserScriptNode[] _nodes;\n\t\t// Effect name : Effect " +
                    "type id\n\t\tprivate Dictionary<string, Guid> _effects = new Dictionary<string, Gui" +
                    "d>();\n\n\t\tpublic ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ClassName));
            
            #line default
            #line hidden
            this.Write("() {\n\t\t\t_nodes = VixenSystem.Nodes.Select(x => new UserScriptNode(x)).ToArray();\n" +
                    "");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"

	foreach(string effectName in Effects.Keys) {

            
            #line default
            #line hidden
            this.Write("\n\t\t\t_effects[\"");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(effectName));
            
            #line default
            #line hidden
            this.Write("\"] = new Guid(\"");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Effects[effectName].TypeId));
            
            #line default
            #line hidden
            this.Write("\");\n");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
 } 
            
            #line default
            #line hidden
            this.Write(@"		
		}

		public void PreRenderEffect(EffectNode effectNode) {
			if(effectNode != null) {
				effectNode.Effect.PreRender();
			}
		}

		public void PreRenderEffects(IEnumerable<EffectNode> effectNodes) {
			if(effectNodes != null) {
				effectNodes.AsParallel().ForAll(x => x.Effect.PreRender());
			}
		}

		public void Execute(EffectNode effectNode) {
			if(effectNode != null) {
				Sequence.InsertData(effectNode);
			}
		}

		public void Execute(IEnumerable<EffectNode> effectNodes) {
			if(effectNodes != null) {
				Sequence.InsertData(effectNodes);
			}
		}

		public EffectNode CreateEffectNode(string effectName, IEnumerable<UserScriptNode> targetNodes, TimeSpan timeSpan, params object[] args) {
			Guid effectId;
			if(_effects.TryGetValue(effectName, out effectId)) {
				IEffectModuleInstance effect = Vixen.Services.ApplicationServices.Get<IEffectModuleInstance>(effectId);
				effect.TimeSpan = timeSpan;
				effect.TargetNodes = targetNodes.Select(x => x.Node).ToArray();
				effect.ParameterValues = args;
				return new EffectNode(effect, TimeSpan.Zero);
			}

			return null;
		}
		
// == Effects ==

");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"

	foreach(string effectName in Effects.Keys) {
		IEffectModuleDescriptor effect = Effects[effectName];
		string effectParameters =
			string.Join(", ",
			(from parameter in effect.Parameters
			select parameter.Type.ToString() + " " + _Mangle(parameter.Name)).ToArray());
		string parameterNames =
			string.Join(", ",
			(from parameter in effect.Parameters
			select _Mangle(parameter.Name)).ToArray());

            
            #line default
            #line hidden
            this.Write("\n\t\t// Original name: ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(effect.EffectName));
            
            #line default
            #line hidden
            this.Write("\n\t\tpublic void ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(effectName));
            
            #line default
            #line hidden
            this.Write("(IEnumerable<UserScriptNode> targetNodes, TimeSpan timeSpan, ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(effectParameters));
            
            #line default
            #line hidden
            this.Write(") {\n\t\t\t_InvokeEffect(\"");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(effectName));
            
            #line default
            #line hidden
            this.Write("\", targetNodes, timeSpan, ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(parameterNames));
            
            #line default
            #line hidden
            this.Write(");\n\t\t}\n\n");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\n\n\t\t// == Nodes ==\n\t\t\n");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"

	// This needs to match the node collection that the script created in its constructor.
	ElementNode[] nodes = VixenSystem.Nodes.ToArray();
	List<string> usedNames = new List<string>();
	for(int i=0; i < nodes.Length; i++) { 
            
            #line default
            #line hidden
            this.Write("\n\t\t// Original name: ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(nodes[i].Name));
            
            #line default
            #line hidden
            this.Write("\n\t\tpublic dynamic ");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_Fix(nodes[i].Name, usedNames)));
            
            #line default
            #line hidden
            this.Write(" { \n\t\t\tget { return _nodes[");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(i));
            
            #line default
            #line hidden
            this.Write("]; }\n\t\t}\n");
            
            #line 1 "C:\Git\vixen\Public\Modules\Script\CSharp\CSharp_ScriptFramework.tt"
 } 
            
            #line default
            #line hidden
            this.Write(@"

		public UserScriptNode[] Nodes {
			get { return _nodes; }
		}

		private void _InvokeEffect(string effectName, IEnumerable<UserScriptNode> targetNodes, TimeSpan timeSpan, params object[] args) {
			Execute(CreateEffectNode(effectName, targetNodes, timeSpan, args));
		}
	}
}
");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public class CSharp_ScriptFrameworkBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
