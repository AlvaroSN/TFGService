using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TFGService
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        byte TryAccess(Access value);

        [OperationContract]
        string Result(byte x);

        [OperationContract]
        void UpdateInfoHash(Access access, String ip, InfoHash info);


    }

    // Utilice un contrato de datos, como se ilustra en el ejemplo siguiente, para agregar tipos compuestos a las operaciones de servicio.

    [DataContract]
    public class Access
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Service { get; set; }
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string IP { get; set; }
    }

}
