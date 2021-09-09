using jba.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace jba.Services
{
    public class JBAReader
    {
        private string _filename;
        private double[] _long = new double[2];
        private double[] _lat = new double[2];
        private double[] _gridXY = new double[2];
        private double[] _startYear = new double[2];
        private double[] _gridRef = new double[2];
        private bool _headerValuesRead = false;
        private double _currentlyProcessingYear = 0;

        public List<PrecipitationData> RainData = new List<PrecipitationData>();

        private ProcessFile _fileProcessing;

        protected Dictionary<FileValues, Func<ProcessFile>> _readFileMethod = new Dictionary<FileValues, Func<ProcessFile>>()
            {
                { FileValues.GridXY, () => new ReadGridXY()  },
                { FileValues.Latitude, () => new ReadLatitude()  },
                { FileValues.Longitude, () => new ReadLongitude()  },
                { FileValues.Years, () => new ReadYears()  },
                { FileValues.GridRef, () => new ReadGridRef()  },
                { FileValues.Data, () => new ReadData()  },
            };

        public JBAReader(string filename = null)
        {
            _filename = filename;
        }

        public void RetrieveData()
        {
            if (string.IsNullOrEmpty(_filename))
                return;

            if (!File.Exists(_filename))
                return;

            var allLines = File.ReadLines(_filename);

            foreach (var line in allLines)
            {
                if (!_headerValuesRead)
                    ProcessHeaderItems(line);

                ProcessGridRef(line);

                _fileProcessing = _readFileMethod[FileValues.Data]();
                List<int> values = _fileProcessing.ReadDataLine(line);
                ProcessPrecipitationData(values);
            }
        }

        /// <summary>
        /// Read all of the header items.
        /// </summary>
        /// <param name="line"></param>
        private void ProcessHeaderItems(string line)
        {
            if (line.Contains("[Long="))
            {
                _fileProcessing = _readFileMethod[FileValues.Longitude]();
                _long = _fileProcessing.ReadFile(line);
            }

            if (line.Contains("[Lati="))
            {
                _fileProcessing = _readFileMethod[FileValues.Latitude]();
                _lat = _fileProcessing.ReadFile(line);
            }

            if (line.Contains("[Grid X,Y="))
            {
                _fileProcessing = _readFileMethod[FileValues.GridXY]();
                _gridXY = _fileProcessing.ReadFile(line);
            }

            if (line.Contains("[Years="))
            {
                _fileProcessing = _readFileMethod[FileValues.Years]();
                _startYear = _fileProcessing.ReadFile(line);

                _headerValuesRead = true;
            }
        }

        /// <summary>
        /// Read the grid ref data which is always before the block of precipitation data.
        /// </summary>
        /// <param name="line"></param>
        private void ProcessGridRef(string line)
        {
            if (line.Contains("Grid-ref="))
            {
                _fileProcessing = _readFileMethod[FileValues.GridRef]();
                _gridRef = _fileProcessing.ReadFile(line);

                _currentlyProcessingYear = _startYear[0];
            }
        }

        /// <summary>
        /// Read the precipitation data
        /// </summary>
        /// <param name="values"></param>
        private void ProcessPrecipitationData(List<int> values)
        {
            if (values != null)
            {
                int month = 1;
                foreach (var v in values)
                {
                    PrecipitationData temp = new PrecipitationData
                    {
                        Date = new DateTime(Convert.ToInt32(_currentlyProcessingYear), month, 01),
                        Value = v,
                        Xref = _gridRef[0],
                        Yref = _gridRef[1]
                    };
                    RainData.Add(temp);
                    month++;
                }

                _currentlyProcessingYear++;
            }
        }
    }
}