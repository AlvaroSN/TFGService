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

    }

    // Utilice un contrato de datos, como se ilustra en el ejemplo siguiente, para agregar tipos compuestos a las operaciones de servicio.

    [DataContract]
    public class Access
    {
        [DataMember]
        //Tipo de acceso: por botón, por lista o por URL
        public string Type { get; set; }
        [DataMember]
        //Nombre de la aplicación alojada en el servidor a la que se accede
        public string App { get; set; }
        [DataMember]
        //ID de sesión de acceso al servidor
        public string ID { get; set; }
        [DataMember]
        //Dirección IP del equipo que intenta acceder el servidor
        public string IP { get; set; }
    }

}
