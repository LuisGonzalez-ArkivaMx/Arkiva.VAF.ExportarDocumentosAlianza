using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFiles.VAF.Configuration;
using System.Runtime.Serialization;

namespace Arkiva.VAF.ExportarDocumentosAlianza
{
    [DataContract]
    public class Configuration
    {
        [DataMember]
        [JsonConfEditor(
            Label = "Directorio Chronoscan",
            HelpText = "Directorio donde se almacenaran los documentos extraidos del servidor Alianza para ser procesados por Chronoscan")]
        public string DirectorioChronoscan { get; set; }

        [DataMember]
        [MFPropertyDef]
        [JsonConfEditor(HelpText = "Definicion de Propiedad del Titulo de Documento", Label = "Propiedad Titulo Documento", IsRequired = true)]
        public MFIdentifier Pd_TituloDocumento { get; set; }

        [DataMember]
        [MFPropertyDef]
        [JsonConfEditor(HelpText = "Definicion de Propiedad del Estado de Procesamiento", Label = "Propiedad Estado de Procesamiento", IsRequired = true)]
        public MFIdentifier Pd_EstadoProcesamiento { get; set; }

        [DataMember]
        [MFPropertyDef]
        [JsonConfEditor(HelpText = "Definicion de Propiedad de CURP", Label = "Propiedad CURP", IsRequired = true)]
        public MFIdentifier Pd_Curp { get; set; }

        [DataMember]
        [MFPropertyDef]
        [JsonConfEditor(HelpText = "Definicion de Propiedad de ", Label = "Propiedad Caja", IsRequired = true)]
        public MFIdentifier Pd_Caja { get; set; }
    }
}
