using System;
using System.Threading;

namespace WebSocketSharp {
  internal class RttMeasure {

    #region Private Fields

    private long      _lastPingTimestamp;
    private Timer     _pingSender;
    private long      _rtt;
    private WebSocket _ws;

    #endregion

    #region Public Constructors

    public RttMeasure (WebSocket ws) {
      _ws = ws;
    }

    #endregion

    #region Public Properties

    public long Rtt {
      get {
        return _rtt;
      }
    }

    #endregion

    #region Private Methods

    private void onPong (object sender, EventArgs e) {
      _rtt = DateTime.Now.Ticks / 10000 - _lastPingTimestamp;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start measurement of the WebSocket RTT.
    /// </summary>
    public void Start () {
      _pingSender = new Timer((object o) => {

        bool success = _ws.Client
                       ? _ws.Ping (WsFrame.CreatePingFrame (Mask.MASK).ToByteArray (), onPong)
                       : _ws.Ping (WsFrame.EmptyUnmaskPingData, onPong);

        if (success) {
          _lastPingTimestamp = DateTime.Now.Ticks / 10000; // Millseconds
        }
      }, null, 0, 5000);
    }

    /// <summary>
    /// Stop measurement of the WebSocket RTT.
    /// </summary>
    public void Stop () {
      _pingSender.Dispose();
    }

    #endregion
  }
}
