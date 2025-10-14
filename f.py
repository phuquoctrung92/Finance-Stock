import sys
import yfinance as yf

if len(sys.argv) < 2:
    sys.exit(1)
ticker = yf.Ticker(sys.argv[1])

df = ticker.history(period="2d", interval="1d")
df.tail().to_csv(sys.argv[1]+".csv", encoding="utf-8")