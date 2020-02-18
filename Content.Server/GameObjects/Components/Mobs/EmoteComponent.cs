using Robust.Shared.Audio;
using Robust.Shared.GameObjects;

using Content.Shared.Audio;
using Robust.Shared.Interfaces.Random;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using System.Collections.Generic;

using YamlDotNet.RepresentationModel;
using Content.Shared.Interfaces;

using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Interfaces.Serialization;

namespace Content.Server.GameObjects.Components.Sound
{
    /// <summary>
    ///     Mobs will do special emotes if they have this component.
    /// </summary>
    [RegisterComponent]
    public class EmoteComponent : Component
    {
#pragma warning disable 649
        [Dependency] private readonly IPrototypeManager _prototypeManager;
        [Dependency] private readonly IRobustRandom _emoteRandom;
#pragma warning restore 649

        private IEnumerable<EmotePrototype> _emotes;

        public override string Name => "EmoteComponent";


        /// <summary>
        /// play emote and return a string of text describing the emote
        /// </summary>
        public string PlayEmote(string emote)
        {
            var output = emote;

            if (!string.IsNullOrWhiteSpace(emote))
            {
                var soundToPlay = emote;

                if (_prototypeManager.TryIndex(emote, out EmotePrototype proto))
                {
                    output = _emoteRandom.Pick(proto.Output);
                    soundToPlay = proto.SoundCollectionID;

                    foreach (var e in proto.Effect)
                    {
                        output = e.PlayEffect(Owner, output);
                    }

                    var soundCollection = _prototypeManager.Index<SoundCollectionPrototype>(soundToPlay);
                    if (soundCollection != null)
                    {
                        var file = _emoteRandom.Pick(soundCollection.PickFiles);
                        Owner.GetComponent<SoundComponent>().Play(file, AudioParams.Default.WithVolume(4f));
                    }
                }
            }

            return output;
        }

    }
}


namespace Content.Server.GameObjects
{
    [Prototype("emote")]
    public class EmotePrototype : IPrototype, IIndexedPrototype
    {
#pragma warning disable 649
        [Dependency] private readonly IModuleManager _moduleManager;
#pragma warning restore 649

        private string _id;
        private List<string> _output;
        private string _soundCollectionID;
        private List<IEmoteEffect> _effect;

        public string ID => _id;
        public List<string> Output => _output;
        public string SoundCollectionID => _soundCollectionID;
        public List<IEmoteEffect> Effect => _effect;

        public EmotePrototype()
        {
            IoCManager.InjectDependencies(this);
        }


        public void LoadFrom(YamlMappingNode mapping)
        {
            var serializer = YamlObjectSerializer.NewReader(mapping);

            serializer.DataField(ref _id, "id", string.Empty);

            if (_moduleManager.IsServerModule)
                serializer.DataField(ref _output, "output", new List<string> { });
            else
                _output = new List<string> { };

            serializer.DataField(ref _soundCollectionID, "soundID", string.Empty);

            if (_moduleManager.IsServerModule)
                serializer.DataField(ref _effect, "effect", new List<IEmoteEffect> { });
            else
                _effect = new List<IEmoteEffect> { };
        }
    }
}


namespace Content.Server.GameObjects
{
    public interface IEmoteEffect : IExposeData
    {
        string PlayEffect(IEntity emotingEntity, string output);
    }

    class DefaultEmote : IEmoteEffect
    {
        void IExposeData.ExposeData(ObjectSerializer serializer)
        {
            
        }
        string IEmoteEffect.PlayEffect(IEntity emotingEntity, string output)
        {
            return output;
        }
    }

    class FartEmote : IEmoteEffect
    {
        private int _gas;
        public int GasProduced => _gas;

        void IExposeData.ExposeData(ObjectSerializer serializer)
        {
            serializer.DataField(ref _gas, "gas", 0); //just an example for now :)
        }
        string IEmoteEffect.PlayEffect(IEntity emotingEntity, string output)
        {
            return output;// + " in the floor's face"; //todo : fart collision overlap whatever checks
        }
    }
}

