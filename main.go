package main

import (
	"github.com/azyobuzin/img.azyobuzi.net/imgazyobuzi"
	"log"
	"os"
	"strconv"
)

func main() {
	logger := log.New(os.Stdout, "[img.azyobuzi.net] ", log.LstdFlags)

	portStr := os.Getenv("PORT") //Set by gin
	var port int
	if portStr == "" {
		port = 61482
	} else {
		port, _ = strconv.Atoi(portStr)
	}

	configFile := os.Getenv("IMGAZYOBUZI_CONFIG")
	if configFile == "" {
		configFile = "/etc/imgazyobuziv3.json"
	}

	ctx, err := imgazyobuzi.NewContextFromFile(configFile, port)
	if err != nil {
		logger.Panic(err)
	}

	logger.Printf("Running on %d\n", ctx.Port)

	ctx.Run()
}
