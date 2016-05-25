#pragma once

#include <stdio.h>
#include <avr/io.h>

static inline void uart_init()
{
	UBRR0 = 0x033;		// Baud: 38400 @ 16MHz

	UCSR0A = 1 << U2X0;
	UCSR0B = 1 << TXEN0 | 1 << RXEN0;	//enable duplex
	UCSR0C = 1 << UCSZ00 | 1 << UCSZ01; //8-N-1
}

static inline uint8_t uart_available()
{
	return UCSR0A & (1 << RXC0);
}

static inline void uart_putchar(uint8_t c)
{
	while ((UCSR0A & (1 << UDRE0)) == 0) ;
	UDR0 = c;
}

static inline uint8_t uart_getchar()
{
	while ((UCSR0A & (1 << RXC0)) == 0) ;			// wait till data
	return UDR0;
}
