using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
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

            while ( root is { parent: { } } )
            {
                root = root.parent;
            }

            m_embedButton = new( () => Client.Embed( m_selectedPackageInfo.name ) )
            {
                text = "Embed",
            };

            var removeButton = root.FindElement( x => x.name == "PackageRemoveCustomButton" );
            removeButton.parent.Insert( 0, m_embedButton );

            m_isInitialized = true;
        }

        void IPackageManagerExtension.OnPackageAddedOrUpdated( PackageInfo packageInfo )
        {
        }

        void IPackageManagerExtension.OnPackageRemoved( PackageInfo packageInfo )
        {
        }
    }
}