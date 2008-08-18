// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Ian Maclean (imaclean@gmail.com)
// Jaroslaw Kowalski (jkowalski@users.sourceforge.net)
// Gert Driesen (gert.driesen@ardatis.com)

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Configuration;
using NAnt.Core.Types;
using NAnt.Core.Util;

//
// This file defines functions for the NAnt category. 
// 
// Please note that property::get-value() is defined in ExpressionEvaluator 
// class because it needs the intimate knowledge of the expressione evaluation stack. 
// Other functions should be defined here.
// 

namespace NAnt.Core.Functions {
    [FunctionSet("nant", "NAnt")]
    public class NAntFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public NAntFunctions(Project project, PropertyDictionary properties) : base(project, properties) {
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Gets the base directory of the appdomain in which NAnt is running.
        /// </summary>
        /// <returns>
        /// The base directory of the appdomain in which NAnt is running.
        /// </returns>
        [Function("get-base-directory")]
        public string GetBaseDirectory() {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Gets the NAnt assembly.
        /// </summary>
        /// <returns>
        /// The NAnt assembly.
        /// </returns>
        [Function("get-assembly")]
        public Assembly GetAssembly() {
            Assembly assembly = Assembly.GetEntryAssembly();
            // check if NAnt was launched as a console application
            if (assembly.GetName().Name != "NAnt") {
                // NAnt is being used as a class library, so return the 
                // NAnt.Core assembly
                assembly = Assembly.GetExecutingAssembly();
            }
            return assembly;
        }

        /// <summary>
        /// Searches the probing paths of the current target framework for the
        /// specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to search for.</param>
        /// <returns>
        /// The absolute path to <paramref name="fileName" /> if found in one of the
        /// configured probing; otherwise, an error is reported.
        /// </returns>
        /// <exception cref="FileNotFoundException"><paramref name="fileName" /> could not be found in the configured probing paths.</exception>
        /// <remarks>
        ///   <para>
        ///   The configured probing paths are scanned recursively in the order
        ///   in which they are defined in the framework configuration.
        ///   </para>
        ///   <para>
        ///   The file name to search should include the extension.
        ///   </para>
        /// </remarks>
        /// <example>
        ///   <para>
        ///   Compile an assembly referencing the <c>nunit.framework</c> assembly
        ///   for the current target framework that is shipped as part of the
        ///   NAnt distribution.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <csc target="library" output="NAnt.Core.Tests.dll">
        ///     <sources basedir="NAnt.Core">
        ///         <include name="**/*.cs" />
        ///     </sources>
        ///     <references>
        ///         <include name="NAnt.Core.dll" />
        ///         <include name="${framework::get-lib-path('nunit.framework.dll')}" />
        ///     </references>
        /// </csc>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("scan-probing-paths")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string ScanProbingPaths(string fileName) {
            string libPath = null;

            FrameworkInfo fi = Project.TargetFramework;
            if (fi.Runtime != null) {
                string[] probingPaths = fi.Runtime.ProbingPaths.GetDirectories();
                libPath = FileUtils.ResolveFile(probingPaths, fileName, true);
            }

            if (libPath == null) {
                throw new FileNotFoundException (string.Format (CultureInfo.InvariantCulture,
                    "\"{0}\" could not be found in any of the configured " +
                    "probing paths.", fileName));
            }
            return libPath;
        }

        #endregion Public Instance Methods
    }

    [FunctionSet("project", "NAnt")]
    public class ProjectFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public ProjectFunctions(Project project, PropertyDictionary properties) : base(project, properties) {
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Gets the name of the current project.
        /// </summary>
        /// <returns>
        /// The name of the current project, or an empty <see cref="string" />
        /// if no name is specified in the build file.
        /// </returns>
        [Function("get-name")]
        public string GetName() {
            return StringUtils.ConvertNullToEmpty(Project.ProjectName);
        }

        /// <summary>
        /// Gets the <see cref="Uri" /> form of the build file.
        /// </summary>
        /// <returns>
        /// The <see cref="Uri" /> form of the build file, or 
        /// an empty <see cref="string" /> if the project is not file backed.
        /// </returns>
        [Function("get-buildfile-uri")]
        public string GetBuildFileUri() {
            if (Project.BuildFileUri != null) {
                return Project.BuildFileUri.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the local path to the build file.
        /// </summary>
        /// <returns>
        /// The local path of the build file, or an empty <see cref="string" />
        /// if the project is not file backed.
        /// </returns>
        [Function("get-buildfile-path")]
        public string GetBuildFilePath() {
            return StringUtils.ConvertNullToEmpty(Project.BuildFileLocalName);
        }

        /// <summary>
        /// Gets the name of the target that will be executed when no other 
        /// build targets are specified.
        /// </summary>
        /// <returns>
        /// The name of the target that will be executed when no other build
        /// targets are specified, or an empty <see cref="string" /> if no
        /// default target is defined for the project.
        /// </returns>
        [Function("get-default-target")]
        public string GetDefaultTarget() {
            return StringUtils.ConvertNullToEmpty(Project.DefaultTargetName);
        }

        /// <summary>
        /// Gets the base directory of the current project.
        /// </summary>
        /// <returns>
        /// The base directory of the current project.
        /// </returns>
        [Function("get-base-directory")]
        public string GetBaseDirectory() {
            return Project.BaseDirectory;
        }

        #endregion Public Instance Methods
    }

    [FunctionSet("target", "NAnt")]
    public class TargetFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public TargetFunctions(Project project, PropertyDictionary properties) : base(project, properties) {
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Checks whether the specified target exists.
        /// </summary>
        /// <param name="name">The target to test.</param>
        /// <returns>
        /// <see langword="true" /> if the specified target exists; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <example>
        ///   <para>
        ///   Execute target &quot;clean&quot;, if it exists.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <if test="${target::exists('clean')}">
        ///     <call target="clean" />
        /// </if>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("exists")]
        public bool Exists(string name) {
            return (Project.Targets.Find(name) != null);
        }

        /// <summary>
        /// Gets the name of the target being executed.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that contains the name of the target
        /// being executed.
        /// </returns>
        /// <exception cref="InvalidOperationException">No target is being executed.</exception>
        [Function("get-current-target")]
        public string GetCurrentTarget() {
            Target target = Project.CurrentTarget;
            if (target == null) {
                throw new InvalidOperationException("No target is being executed.");
            }
            return target.Name;
        }

        /// <summary>
        /// Checks whether the specified target has already been executed.
        /// </summary>
        /// <param name="name">The target to test.</param>
        /// <returns>
        /// <see langword="true" /> if the specified target has already been 
        /// executed; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentException">Target <paramref name="name" /> does not exist.</exception>
        [Function("has-executed")]
        public bool HasExecuted(string name) {
            Target target = Project.Targets.Find(name);
            if (target == null) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    ResourceUtils.GetString("NA1097"), name));
            }

            return target.Executed;
        }

        #endregion Public Instance Methods
    }

    [FunctionSet("task", "NAnt")]
    public class TaskFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public TaskFunctions(Project project, PropertyDictionary properties) : base(project, properties) {
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Checks whether the specified task exists.
        /// </summary>
        /// <param name="name">The task to test.</param>
        /// <returns>
        /// <see langword="true" /> if the specified task exists; otherwise,
        /// <see langword="false" />.
        /// </returns>
        [Function("exists")]
        public bool Exists(string name) {
            return TypeFactory.TaskBuilders.Contains(name);
        }

        /// <summary>
        /// Returns the <see cref="Assembly" /> from which the specified task
        /// was loaded.
        /// </summary>
        /// <param name="name">The name of the task to get the <see cref="Assembly" /> of.</param>
        /// <returns>
        /// The <see cref="Assembly" /> from which the specified task was loaded.
        /// </returns>
        /// <exception cref="ArgumentException">Task <paramref name="name" /> is not available.</exception>
        [Function("get-assembly")]
        public Assembly GetAssembly(string name) {
            TaskBuilder task = TypeFactory.TaskBuilders[name];
            if (task == null) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    ResourceUtils.GetString("NA1099"), name));
            }

            return task.Assembly;
        }

        #endregion Public Instance Methods
    }

    [FunctionSet("property", "NAnt")]
    public class PropertyFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public PropertyFunctions(Project project, PropertyDictionary properties) : base(project, properties) {
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Checks whether the specified property exists.
        /// </summary>
        /// <param name="name">The property to test.</param>
        /// <returns>
        /// <see langword="true" /> if the specified property exists; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <example>
        ///   <para>
        ///   Execute a set of tasks if the &quot;build.debug&quot; property
        ///   exists.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <if test="${property::exists('build.debug')}">
        ///     <echo message="Starting debug build" />
        ///     <call target="init-debug" />
        ///     <call target="build" />
        /// </if>
        ///     ]]>
        ///   </code>
        /// </example>
        [Function("exists")]
        public bool Exists(string name) {
            return Project.Properties.Contains(name);
        }

        /// <summary>
        /// Checks whether the specified property is read-only.
        /// </summary>
        /// <param name="name">The property to test.</param>
        /// <returns>
        /// <see langword="true" /> if the specified property is read-only; 
        /// otherwise, <see langword="false" />.
        /// </returns>
        /// <example>
        ///   <para>Check whether the &quot;debug&quot; property is read-only.</para>
        ///   <code>property::is-readonly('debug')</code>
        /// </example>
        /// <exception cref="ArgumentException">Property <paramref name="name" /> has not been set.</exception>
        [Function("is-readonly")]
        public bool IsReadOnly(string name) {
            if (!Project.Properties.Contains(name)) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, 
                    ResourceUtils.GetString("NA1053"), name));
            }

            return Project.Properties.IsReadOnlyProperty(name);
        }

        /// <summary>
        /// Checks whether the specified property is a dynamic property.
        /// </summary>
        /// <param name="name">The property to test.</param>
        /// <returns>
        /// <see langword="true" /> if the specified property is a dynamic
        /// property; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentException">Property <paramref name="name" /> has not been set.</exception>
        /// <example>
        ///   <para>
        ///   Check whether the &quot;debug&quot; property is a dynamic property.
        ///   </para>
        ///   <code>property::is-dynamic('debug')</code>
        /// </example>
        [Function("is-dynamic")]
        public bool IsDynamic(string name) {
            if (!Project.Properties.Contains(name)) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, 
                    ResourceUtils.GetString("NA1053"), name));
            }

            return Project.Properties.IsDynamicProperty(name);
        }

        #endregion Public Instance Methods
    }

    [FunctionSet("platform", "NAnt")]
    public class PlatformFunctions : FunctionSetBase {
        #region Public Instance Constructors

        public PlatformFunctions(Project project, PropertyDictionary properties) : base(project, properties) {
        }

        #endregion Public Instance Constructors

        #region Public Instance Methods

        /// <summary>
        /// Gets the name of the platform on which NAnt is running.
        /// </summary>
        /// <returns>
        /// The name of the platform on which NAnt is running.
        /// </returns>
        [Function("get-name")]
        public string GetName() {
            return Project.PlatformName;
        }

        #endregion Public Instance Methods

        #region Public Static Methods

        /// <summary>
        /// Checks whether NAnt is running on the win32 platform.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if NAnt is running on the win32 platform;
        /// otherwise, <see langword="false" />.
        /// </returns>
        [Function("is-win32")]
        public static bool IsWin32() {
            return PlatformHelper.IsWin32;
        }

        /// <summary>
        /// Checks whether NAnt is running on unix.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if NAnt is running on unix;
        /// otherwise, <see langword="false" />.
        /// </returns>
        [Function("is-unix")]
        public static bool IsUnix() {
            return PlatformHelper.IsUnix;
        }
        
        #endregion Public Static Methods
    }
}
