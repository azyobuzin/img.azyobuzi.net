package main

import (
	"fmt"
	"github.com/codegangsta/cli"
	"io"
	"log"
	"os"
	"os/exec"
	"os/signal"
	"path"
	"time"
)

func main() {
	app := cli.NewApp()
	app.Name = "bootstrapper"
	app.Usage = "img.azyobuzi.net v3 bootstrapper"
	app.Flags = []cli.Flag{
		cli.IntFlag{"port,p", 63001, "port for the proxy server"},
		cli.IntFlag{"appPort,a", 61482, "port for the Go web server"},
		cli.StringFlag{"log,l", "/var/log/imgazyobuziv3", "directory for logs"},
	}
	app.Action = action
	app.Run(os.Args)
}

func action(c *cli.Context) {
	var writer io.Writer = os.Stdout
	logPath := c.String("log")
	if logPath != "" {
		err := os.MkdirAll(logPath, 0666)
		if err == nil {
			file, err := os.Create(path.Join(logPath, time.Now().Format("20060102_150405.log")))
			if err == nil {
				defer file.Close()
				writer = io.MultiWriter(writer, file)
			} else {
				fmt.Printf("[Bootstrapper] %v\n", err)
			}
		} else {
			fmt.Printf("[Bootstrapper] %v\n", err)
		}
	}

	logger := log.New(writer, "[Bootstrapper] ", log.LstdFlags)
	logger.Println("Running")

	gopath := os.Getenv("GOPATH")
	cmd := exec.Command(path.Join(gopath, "bin", "gin"), "-p", c.String("port"), "-a", c.String("appPort"))
	cmd.Dir = path.Join(gopath, "src", "github.com", "azyobuzin", "img.azyobuzi.net")
	stdout, _ := cmd.StdoutPipe()
	stderr, _ := cmd.StderrPipe()
	err := cmd.Start()
	if err != nil {
		logger.Fatalf("Failed: %v\n", err)
	}
	logger.Println("Started gin")

	go io.Copy(writer, stdout)
	go io.Copy(writer, stderr)

	go func() {
		c := make(chan os.Signal)
		signal.Notify(c, os.Interrupt, os.Kill)
		s := <-c
		logger.Printf("Signal: %v\n", s)
		err := cmd.Process.Signal(s)
		if err != nil {
			logger.Printf("Send signal: %v\n", err)
		}
	}()

	logger.Printf("Shutting down: %v\n", cmd.Wait())
}
