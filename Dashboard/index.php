<!doctype html>
<html lang="en">
<head>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, initial-scale=1">
	<title>example-aggregate-functions-and-grouping-count-with-group-by- php mysql examples | w3resource</title>
	<meta name="description" content="example-aggregate-functions-and-grouping-count-with-group-by- php mysql examples | w3resource">
	<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css">
</head>
<body>
	<div class="container">
		<div class="row">
			<div class="col-md-12">
				<h2>Taking an account of how many times a tool has been on hold:</h2>
				<table class='table table-bordered'>
				<tr>
				<th>Tool name</th><th>Number of holds</th>
				</tr>
				<?php
					$servername = "localhost";
					$username = "root";
					$password = "root";
					$dbname = "analytics_database";
					$port = "5000";
					$dbh = new PDO("mysql:host=$servername:$port;dbname=$dbname", $username, $password);
					foreach($dbh->query('SELECT EquipName,COUNT(*)
					FROM analytics_database.aggregateddata
					GROUP BY EquipName') as $row) {
						echo "<tr>";
						echo "<td>" . $row['EquipName'] . "</td>";
						echo "<td>" . $row['COUNT(*)'] . "</td>";
						echo "</tr>"; 
					}
				?>
				</tbody></table>
			</div>
			<div class="col-md-12">
				<h2>Taking an account of how many times a recipe has been on hold:</h2>
				<table class='table table-bordered'>
				<tr>
				<th>Recipe name</th><th>Number of holds</th>
				</tr>
				<?php
					$servername = "localhost";
					$username = "root";
					$password = "root";
					$dbname = "analytics_database";
					$port = "5000";
					$dbh = new PDO("mysql:host=$servername:$port;dbname=$dbname", $username, $password);
					foreach($dbh->query('SELECT RecipeName,COUNT(*)
					FROM analytics_database.aggregateddata
					GROUP BY RecipeName') as $row) {
						echo "<tr>";
						echo "<td>" . $row['RecipeName'] . "</td>";
						echo "<td>" . $row['COUNT(*)'] . "</td>";
						echo "</tr>"; 
					}
				?>
				</tbody></table>
			</div>
			<div class="col-md-12">
				<h2>Taking an account of how many times a type of hold has been on hold:</h2>
				<table class='table table-bordered'>
				<tr>
				<th>Type of hold</th><th>Number of holds</th>
				</tr>
				<?php
					$servername = "localhost";
					$username = "root";
					$password = "root";
					$dbname = "analytics_database";
					$port = "5000";
					$dbh = new PDO("mysql:host=$servername:$port;dbname=$dbname", $username, $password);
					foreach($dbh->query('SELECT HoldType,COUNT(*)
					FROM analytics_database.aggregateddata
					GROUP BY HoldType') as $row) {
						echo "<tr>";
						echo "<td>" . $row['HoldType'] . "</td>";
						echo "<td>" . $row['COUNT(*)'] . "</td>";
						echo "</tr>"; 
					}
				?>
				</tbody></table>
			</div>
		</div>
	</div>
</body>
</html>