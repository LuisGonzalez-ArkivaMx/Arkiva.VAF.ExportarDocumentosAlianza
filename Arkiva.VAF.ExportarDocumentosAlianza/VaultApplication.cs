using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Core;
using MFilesAPI;

namespace Arkiva.VAF.ExportarDocumentosAlianza
{
    /// <summary>
    /// The entry point for this Vault Application Framework application.
    /// </summary>
    /// <remarks>Examples and further information available on the developer portal: http://developer.m-files.com/. </remarks>
    public class VaultApplication
        : ConfigurableVaultApplicationBase<Configuration>
    {
        [EventHandler(MFEventHandlerType.MFEventHandlerAfterFileUpload, Class = "CL.DocumentoSinClasificar")] //[EventHandler(MFEventHandlerType.MFEventHandlerAfterCheckInChangesFinalize, Class = "CL.DocumentoSinClasificar")]
        public void ExportacionDeDocumentosSinClasificar(EventHandlerEnvironment env)
        {
            int iEstadoProcesamiento = 0;
            var archivosAEliminar = new List<string>();
            var oPropertyValues = new PropertyValues();

            try
            {
                oPropertyValues = env.Vault.ObjectPropertyOperations.GetProperties(env.ObjVer);

                var sTituloDocumento = oPropertyValues
                    .SearchForPropertyEx(Configuration.Pd_TituloDocumento, true)
                    .TypedValue
                    .GetValueAsLocalizedText();

                var sCurp = oPropertyValues.SearchForPropertyEx(Configuration.Pd_Curp, true).TypedValue.GetValueAsLocalizedText();

                var sCaja = oPropertyValues.SearchForPropertyEx(Configuration.Pd_Caja, true).TypedValue.GetValueAsLocalizedText();

                var oObjVerEx = env.ObjVerEx;
                var oObjectFiles = oObjVerEx.Info.Files;
                IEnumerator enumerator = oObjectFiles.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    ObjectFile oFile = (ObjectFile)enumerator.Current;

                    var iObjectID = env.ObjVerEx.ID;

                    var sObjectGUID = oObjVerEx.Info.ObjectGUID;

                    string sFilePath = SysUtils.GetTempFileName(".tmp");

                    archivosAEliminar.Add(sFilePath);

                    // Gets the latest version of the specified file
                    FileVer fileVer = oFile.FileVer;

                    // Download the file to a temporary location
                    env.Vault.ObjectFileOperations.DownloadFile(oFile.ID, fileVer.Version, sFilePath);

                    var sFileName = oFile.GetNameForFileSystem();

                    /////////////////////////////////////////////////////////
                    //var nombreConcat = iObjectID + " - " + sFileName;

                    //var nombreArchivo = Path.Combine(@"C:\TempMFilesVAF", nombreConcat);

                    //File.Copy(sFilePath, nombreArchivo);
                    /////////////////////////////////////////////////////////

                    var sDelimitador = ".";

                    int iIndex = sFileName.LastIndexOf(sDelimitador);

                    var sExtension = sFileName.Substring(iIndex + 1);

                    var sNombreLote = sCurp + " - " + sCaja;

                    // Generar directorio por Lote (CURP + Caja)
                    string sFilePathLote = Path.Combine(Configuration.DirectorioChronoscan, sNombreLote);

                    // Verificar el directorio recien generado 
                    if (!Directory.Exists(sFilePathLote))
                    {
                        Directory.CreateDirectory(sFilePathLote);
                    }

                    // Nombre concatenado para el archivo
                    var sFileNameConcatenado = iObjectID + " - " + sObjectGUID + " - " + sFileName;

                    // Directorio completo del documento
                    string sNewFilePath = Path.Combine(sFilePathLote, sFileNameConcatenado);

                    // Copiar el documento en el nuevo directorio
                    File.Copy(sFilePath, sNewFilePath);

                    if (File.Exists(sNewFilePath))
                    {
                        // Documento Procesado con exito
                        iEstadoProcesamiento = 1;
                    }
                    else
                    {
                        // No Procesado o Termino en Error
                        iEstadoProcesamiento = 2;
                    }                    
                }

                // Actualizar el valor de la propiedad "Estado de Procesamiento" del documento
                var oLookup = new Lookup();
                var oObjID = new ObjID();

                oObjID.SetIDs
                (
                    ObjType: (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument,
                    ID: env.ObjVer.ID
                );

                //var checkedOutObjectVersion = env.Vault.ObjectOperations.CheckOut(oObjID);

                var oPropertyValue = new PropertyValue
                {
                    PropertyDef = Configuration.Pd_EstadoProcesamiento
                };

                oLookup.Item = iEstadoProcesamiento;

                oPropertyValue.TypedValue.SetValueToLookup(oLookup);

                env.Vault.ObjectPropertyOperations.SetProperty
                (
                    ObjVer: env.ObjVer,
                    PropertyValue: oPropertyValue
                );

                //env.Vault.ObjectOperations.CheckIn(checkedOutObjectVersion.ObjVer);

                SysUtils.ReportInfoToEventLog("Exportacion exitosa del documento.");
            }
            catch (Exception ex)
            {
                SysUtils.ReportErrorMessageToEventLog("Error al exportar el documento, ", ex);
            }
            finally
            {
                // Limpiar los archivos temporales.
                foreach (var archivo in archivosAEliminar)
                {
                    File.Delete(archivo);
                }
            }
        }
    }
}