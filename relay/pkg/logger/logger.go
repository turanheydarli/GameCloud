package logger

import "log"

type Logger interface {
	Infow(msg string, keyvals ...interface{})
	Warnw(msg string, keyvals ...interface{})
	Errorw(msg string, keyvals ...interface{})
	Fatalw(msg string, keyvals ...interface{})
}

type StdLogger struct {
	level string
}

func NewStdLogger(level string) Logger {
	return &StdLogger{level: level}
}

func (l *StdLogger) Infow(msg string, keyvals ...interface{}) {
	log.Println(append([]interface{}{"INFO:", msg}, keyvals...)...)
}
func (l *StdLogger) Warnw(msg string, keyvals ...interface{}) {
	log.Println(append([]interface{}{"WARN:", msg}, keyvals...)...)
}
func (l *StdLogger) Errorw(msg string, keyvals ...interface{}) {
	log.Println(append([]interface{}{"ERROR:", msg}, keyvals...)...)
}
func (l *StdLogger) Fatalw(msg string, keyvals ...interface{}) {
	log.Fatal(append([]interface{}{"FATAL:", msg}, keyvals...)...)
}
