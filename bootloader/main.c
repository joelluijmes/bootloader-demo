#include <avr/io.h>
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <util/delay.h>
#include <stdio.h>
#include <string.h>

#include "uart.h"

#define LAST_RECORD 1
#define READY 0x0A
#define RESENT 0x0B
#define ACK 0x0C

typedef struct HexRecord HexRecord;
struct HexRecord
{
	uint8_t code;
	uint8_t len;
	uint16_t address;
	uint8_t type;
	uint8_t* data;
	uint8_t checksum;
};

// computes checksum of hexrecord
static uint8_t compute_checksum(const HexRecord* record)
{
	// Checksum is the two complements sum of the record
	uint8_t checksum = record->len + (uint8_t)(record->address >> 8) + (uint8_t)record->address;
	for (uint8_t i = 0; i < record->len; ++i)
		checksum += record->data[i];
	
	return (uint8_t)(-checksum);			// negating gives two's complement
}

// reads hex record from uart
static void receive_record(HexRecord* record)
{
	static uint8_t data_buf[16];		// avr-gcc makes records with 16 data bytes
	
	uint8_t code;						// wait till we get the start code ':'
	while ((code = uart_getchar()) != ':') ;
	
	record->code = code;				// reads code
	record->len = uart_getchar();		// reads len
	record->address = uart_getchar()| uart_getchar() << 8;	// reads the address
	record->type = uart_getchar();		// reads the type
	record->data = data_buf;			// point to our static local placeholder
	
	for (uint8_t i = 0; i < record->len; ++i)
		data_buf[i] = uart_getchar();	// read the actual data :D
		
	record->checksum = uart_getchar();	// read the checksum
}

// receives page from uart
static uint8_t receive_page(uint8_t* buf)
{
	HexRecord hex_record = {0};
	uint8_t address = 0;						// current address in the page
	
	while (address < SPM_PAGESIZE)				// while we don't have enough bytes for a page
	{								
		uart_putchar(READY);					// send ready command
		receive_record(&hex_record);			// receive one intel hex record from the uart
		if (hex_record.type == LAST_RECORD)
			return 1;							// specify we received the last record
			
		uint8_t checksum = compute_checksum(&hex_record);
		if (checksum != hex_record.checksum)	// if invalid checksum
		{
			uart_putchar(RESENT);				// let the programmer resent this record
			continue;							
		}
		
		// put the received data from the record in our page buffer
		for (uint8_t i = 0; i < hex_record.len; ++i)
			buf[address++] = hex_record.data[i];
	}
	
	return 0;		// this wasn't the last record so we expect more to come :D
}

void program_page(uint32_t page, uint8_t *buf)
{
	boot_page_erase(page);		// erase the page
	boot_spm_busy_wait();// wait for spm instruction
	
	// pages are divided into 16 bit words so fill it at two bytes at a time
	for (uint8_t i = 0; i < SPM_PAGESIZE; i += 2)
	{
		uint16_t word = buf[i] | ((uint16_t)buf[i + 1] << 8);	// LSB First
		boot_page_fill(page + i, word);		// fill the page buffer
	}
	
	boot_page_write(page);		// write the page to flash
	boot_spm_busy_wait();		// wait for the operation to complete
	boot_rww_enable();			// re-enable the RWW section (makes code executable)
}

int main(void)
{
	DDRB = 0x20;			// set led as output
	uart_init();			// initialize the uart
	
	// Right now the bootloader waits indefinitely on any signal from the programmer 
	// this should be replaced by some timeout so that the application runs when it
	// doesn't get updated
	uart_getchar();			
	
	// Fancieh led activity
	for (uint8_t i = 0; i < 10; ++i)
	{
		PINB = 0x20;
		_delay_ms(50);
	}
	_delay_ms(100);
			
	uint8_t page[SPM_PAGESIZE], last = 0;
	uint16_t address = 0;						// address of flash memory
	
	while (!last)
	{
		memset(page, 0, SPM_PAGESIZE);			// clear our buffer
		last = receive_page(page);				// receive page from programmer
		
		program_page(address, page);			// programs the page to the flash
		address += SPM_PAGESIZE;				// increment our address
	}
	
	asm("jmp 0x0000");							// Jump to main program :)
	return 0;
}

