var httpClient = new HttpClient();

var response = await httpClient.GetAsync("https://dramaday.me/?6bb2feb0ae=YzNZSDNCWVdveGJOWkdieFUzdWhVMklHTkp4eDhjVG5MbFBnQ0p3YUUvYUZYZ21YT0E0cHVqdU5kd0sycXlhK2FuU1N4eUFibGRtNU91Z1BrdFZlSURCazgwdjNFLzY4b2UvSUYrYzQ0R1U9");
var text = await response.Content.ReadAsStringAsync();
Console.WriteLine(text);