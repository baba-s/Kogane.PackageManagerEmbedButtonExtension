using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEditorInternal;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Kogane.Internal
{
    [InitializeOnLoad]
    internal sealed class PackageManagerEmbedButtonExtension
        : VisualElement,
          IPackageManagerExtension
    {
        private bool        m_isInitialized;
        private PackageInfo m_selectedPackageInfo;
        private Button      m_embedButton;

        static PackageManagerEmbedButtonExtension()
        {
            var extension = new PackageManagerEmbedButtonExtension();
            PackageManagerExtensions.RegisterExtension( extension );
        }

        VisualElement IPackageManagerExtension.CreateExtensionUI()
        {
            m_isInitialized = false;
            return this;
        }

        void IPackageManagerExtension.OnPackageSelectionChange( PackageInfo packageInfo )
        {
            Initialize();

            m_selectedPackageInfo = packageInfo;

            m_embedButton.SetEnabled( packageInfo is { source: PackageSource.Git or PackageSource.Registry } );
        }

        private void Initialize()
        {
            if ( m_isInitialized ) return;

            VisualElement root = this;

            while ( root != null && root.parent != null )
            {
                root = root.parent;
            }

            m_embedButton = new Button( () => Client.Embed( m_selectedPackageInfo.name ) )
            {
                text = "Embed",
            };

            var removeButton = FindElement( root, x => x.name == "PackageRemoveCustomButton" );
            removeButton.parent.Add( m_embedButton );

            m_isInitialized = true;
        }

        void IPackageManagerExtension.OnPackageAddedOrUpdated( PackageInfo packageInfo )
        {
        }

        void IPackageManagerExtension.OnPackageRemoved( PackageInfo packageInfo )
        {
        }

        private static VisualElement FindElement
        (
            VisualElement             element,
            Func<VisualElement, bool> predicate
        )
        {
            var selected                    = new List<VisualElement>();
            var engineAssemblyPath          = InternalEditorUtility.GetEngineAssemblyPath();
            var engineAssemblyDirectoryName = Path.GetDirectoryName( engineAssemblyPath ).Replace( "\\", "/" );
            var assembly                    = Assembly.LoadFile( $@"{engineAssemblyDirectoryName}\UnityEditor.UIBuilderModule.dll" );
            var type                        = assembly.GetType( "Unity.UI.Builder.VisualElementExtensions" );
            var findElementsRecursiveMethod = type.GetMethod( "FindElementsRecursive", BindingFlags.Static | BindingFlags.NonPublic );

            findElementsRecursiveMethod.Invoke( null, new object[] { element, predicate, selected } );

            return selected.Count == 0 ? null : selected[ 0 ];
        }
    }
}