start C:\Users\kenke\museFiles\batchscripts\init.bat
echo "ZERO"
timeout 10
echo "ONE"
start C:\Users\kenke\museFiles\batchscripts\grabData.bat masterBatchCallTest.csv
echo "HELLO!"
timeout 12
echo "Exiting soon"
taskkill /F /IM muse-player*