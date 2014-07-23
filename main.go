package main

import (
	"github.com/azyobuzin/img.azyobuzi.net/imgazyobuzi"
	"log"
	"os"
	"strconv"
)

func main() {
	logger := log.New(os.Stdout, "[img.azyobuzi.net] ", log.LstdFlags)

	configFile := os.Getenv("IMGAZYOBUZI_CONFIG")
	if configFile == "" {
		configFile = "/etc/imgazyobuziv3.toml"
	}

	ctx, err := imgazyobuzi.NewContextFromFile(configFile)
	if err != nil {
		logger.Panic(err)
	}

	ctx.Logger = logger

	portStr := os.Getenv("PORT")
	if portStr != "" {
		port, _ := strconv.Atoi(portStr)
		ctx.Port = port
	}

	logger.Printf("Running on %d\n", ctx.Port)

	ctx.Run()
}
