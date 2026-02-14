namespace UnityEngine.Toolbox
{
    public static partial class SoundExt
    {
        /// <summary>
        /// Останавливает воспроизведение звуков на игровом объекте и всех его дочерних объектах, содержащих AudioSource.
        /// </summary>
        /// <param name="inst">Игровой объект, на котором нужно остановить звуки.</param>
        public static void StopSounds(GameObject inst)
        {
            if (inst == null)
                return;

            if (inst.GetComponent<AudioSource>() != null)
            {
                if (inst.GetComponent<AudioSource>().isPlaying)
                {
                    inst.GetComponent<AudioSource>().Stop();
                }
            }

            foreach (AudioSource source in inst.GetComponentsInChildren<AudioSource>())
            {
                source.Stop();
            }
        }

        /// <summary>
        /// Приостанавливает воспроизведение звуков на игровом объекте и всех его дочерних объектах, содержащих AudioSource.
        /// </summary>
        /// <param name="inst">Игровой объект, на котором нужно приостановить звуки.</param>
        public static void PauseSounds(GameObject inst)
        {
            if (inst == null)
                return;

            if (inst.GetComponent<AudioSource>() != null)
            {
                if (inst.GetComponent<AudioSource>().isPlaying)
                {
                    inst.GetComponent<AudioSource>().Pause();
                }
            }

            foreach (AudioSource source in inst.GetComponentsInChildren<AudioSource>())
            {
                source.Pause();
            }
        }

        /// <summary>
        /// Запускает воспроизведение звуков на игровом объекте и всех его дочерних объектах, содержащих AudioSource.
        /// </summary>
        /// <param name="inst">Игровой объект, на котором нужно запустить звуки.</param>
        public static void PlaySounds(GameObject inst)
        {
            if (inst == null)
                return;

            if (inst.GetComponent<AudioSource>() != null)
            {
                if (!inst.GetComponent<AudioSource>().isPlaying)
                {
                    inst.GetComponent<AudioSource>().Play();
                }
            }

            foreach (AudioSource source in inst.GetComponentsInChildren<AudioSource>())
            {
                source.Play();
            }
        }

        /// <summary>
        /// Проверяет, воспроизводится ли какой-либо AudioSource на игровом объекте или его дочерних объектах.
        /// </summary>
        /// <param name="inst">Игровой объект для проверки.</param>
        /// <returns>Возвращает `true`, если хотя бы один AudioSource воспроизводит звук, иначе `false`.</returns>
        public static bool IsAudioSourcePlaying(GameObject inst)
        {
            if (inst == null)
                return false;

            if (inst.GetComponent<AudioSource>() != null)
            {
                if (inst.GetComponent<AudioSource>().isPlaying)
                {
                    return true;
                }
            }

            foreach (AudioSource source in inst.GetComponentsInChildren<AudioSource>())
            {
                if (source.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }
    }
}