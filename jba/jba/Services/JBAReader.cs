﻿using jba.Models;
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
        private List<(double gridX, double gridY, DateTime date, int value)> _precipitationData = new List<(double gridX, double gridY, DateTime date, int value)>();

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

        private void ProcessGridRef(string line)
        {
            if (line.Contains("Grid-ref="))
            {
                _fileProcessing = _readFileMethod[FileValues.GridRef]();
                _gridRef = _fileProcessing.ReadFile(line);

                _currentlyProcessingYear = _startYear[0];
            }
        }

        private void ProcessPrecipitationData(List<int> values)
        {
            if (values != null)
            {
                int month = 1;
                foreach (var v in values)
                {
                    _precipitationData.Add((_gridRef[0], _gridRef[1], new DateTime(Convert.ToInt32(_currentlyProcessingYear), month, 01), v));
                    month++;
                }

                _currentlyProcessingYear++;
            }
        }
    }
}