<?php

// Enable error reporting for debugging
error_reporting(E_ALL);
ini_set('display_errors', 1);

date_default_timezone_set('UTC');

require_once 'config.php';

header('Access-Control-Allow-Origin: *');
header('Content-Type: application/json');

// Create connection
$conn = new mysqli(DB_SERVER, DB_USER, DB_PASSWORD, DB_NAME);

// Check connection
if ($conn->connect_error) {
    echo json_encode(["status" => "error", "message" => "Connection failed: " . $conn->connect_error]);
    exit;
}

$amount = $_GET['amount'] ?? null;

if ($amount === null) {
    echo json_encode(["status" => "error", "message" => "No value parameter passed."]);
    exit;
// } else {
//     echo json_encode(["status" => "error", "message" => "Value passed: " .$amount]);
//     exit;
}

// Fetch currency units from the database
$query = "SELECT value, type FROM cash_units ORDER BY value DESC";
$result = $conn->query($query);

if (!$result) {
    echo json_encode(["status" => "error", "message" => "Error fetching cash units: " . $conn->error]);
    exit;
}

$bills = 0;
$coins = 0;
$totalPieces = 0;
$cashBreakdown = [];

// Greedy algorithm to calculate the minimum number of pieces needed
while ($row = $result->fetch_assoc()) {
    $unitValue = (float)$row['value'];
    $count = 0;
    while ($amount >= $unitValue) {
        $amount -= $unitValue;
        $totalPieces++;
        $count++;
    }

    if ($count > 0) {
        $cashBreakdown[] = "{$count}x{$unitValue}€";
        if ($row['type'] == 'bill') {
            $bills += $count;
        } else {
            $coins += $count;
        }
    }

}

// Close the connection
$conn->close();

// Convert array to string, e.g., '2x200€, 1x50€, ...'
$breakdownString = implode(", ", $cashBreakdown);

// Output the results as JSON
echo json_encode([
    "status" => "success",
    "totalPieces" => $totalPieces,
    "bills" => $bills,
    "coins" => $coins,
    "breakdown" => $breakdownString  
]);

?>
