package main

import (
	"fmt"
	"github.com/azyobuzin/img.azyobuzi.net/imgazyobuzi"
	"os"
	"strconv"
)

func main() {
	portStr := os.Getenv("PORT") //Set by gin
	var port int
	if portStr == "" {
		port = 61482
	} else {
		port, _ = strconv.Atoi(portStr)
	}

	newRelicLicenseKey := os.Getenv("IMGAZYOBUZI_NR_LICENSE_KEY")
	newRelicAppName := os.Getenv("IMGAZYOBUZI_NR_APP_NAME")

	fmt.Printf("[img.azyobuzi.net] Running on %d\n", port)

	imgazyobuzi.Run(port, newRelicLicenseKey, newRelicAppName)
}
