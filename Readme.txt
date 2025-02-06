Vi
Thay đổi đường dẫn trong ExcecuteSQL.exe.config như sau
-Thay đổi đường dẫn connection string đến DB cần APPLY file SQL
-Thư mục chứa procedure, function thay đổi tại SqlFilesDirectory

Nếu có file sql chạy thất bại thì sẽ tạo ra một thư mục tên FailedSQL để chứa các procedure đó

En
Modify the path in the ExcecuteSQL.exe.config file as follows:

Change the connection string path to the database (DB) where the SQL file needs to be APPLIED.
Change the folder containing the procedures and functions at SqlFilesDirectory.
If any SQL files fail to run, a folder named FailedSQL will be created to store those procedures.

Ja
ExcecuteSQL.exe.configファイルのパスを以下のように変更してください：

SQLファイルを適用するデータベース（DB）への接続文字列のパスを変更。
SqlFilesDirectoryで、プロシージャやファンクションを含むフォルダを変更。
SQLファイルの実行が失敗した場合、そのプロシージャを保存するためにFailedSQLという名前のフォルダが作成されます。


