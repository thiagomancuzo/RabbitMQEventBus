using System;

namespace Framework.Messaging.EventBus.Events
{
    public abstract class BasicEvent
    {
        protected BasicEvent()
        {
            this.Id = Guid.NewGuid();
            this.CreatedAt = DateTime.Now;
        }

        // TODO: Garça - gostaria de deixar as propriedades abaixo com set privado ou readonly, 
        // mas para possibilitar a atribuição na recepção da mensagem isso não foi possível.
        // Verificar futuras alternativas, ex: inicialização por argumentos do construtor

        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
