using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Komodo.Classes
{
    /// <summary>
    /// A node of data from a parsed document.
    /// </summary>
    public class DataNode
    {
        #region Public-Members

        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The data associated with the key.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The DataType associated with the key-value pair.
        /// </summary>
        public DataType Type { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the DataNode.
        /// </summary>
        public DataNode()
        {

        }

        /// <summary>
        /// Instantiates the DataNode.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data associated with the key.</param>
        /// <param name="type">The DataType associated with the key-value pair.</param>
        public DataNode(string key, object data, DataType type)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            
            Key = key;
            Data = data;
            Type = type;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve the DataType of the supplied value.
        /// </summary>
        /// <param name="val">The object to evaluate.</param>
        /// <returns>DataType.</returns>
        public static DataType TypeFromValue(object val)
        {
            if (val == null) return DataType.Null;

            decimal testDecimal;
            int testInt;
            long testLong;
            bool testBool;

            if (val.ToString().Contains("."))
            {
                if (Decimal.TryParse(val.ToString(), out testDecimal))
                {
                    return DataType.Decimal;
                }
            }

            if (Int32.TryParse(val.ToString(), out testInt))
            {
                return DataType.Integer;
            }

            if (Int64.TryParse(val.ToString(), out testLong))
            {
                return DataType.Long;
            }

            if (Boolean.TryParse(val.ToString(), out testBool))
            {
                return DataType.Boolean;
            }

            return DataType.String;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
